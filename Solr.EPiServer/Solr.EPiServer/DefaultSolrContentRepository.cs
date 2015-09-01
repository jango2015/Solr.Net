using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Globalization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Solr.Client;
using Solr.Client.Linq;
using Solr.EPiServer.Helpers;

namespace Solr.EPiServer
{
    [ServiceConfiguration(ServiceType = typeof(ISolrContentRepository), Lifecycle = ServiceInstanceScope.Hybrid)]
    public class DefaultSolrContentRepository : ISolrContentRepository
    {
        private readonly ISolrConfiguration _solrConfiguration;
        private readonly ILogger _logger;
        private readonly HashSet<string> _mandatoryAttributes = new HashSet<string> { "PageName" };
        private readonly Guid _commerceSiteId = Guid.Empty;

        private const string FieldNameId = "id";
        private const string FieldNameType = "_types_ss";
        private const string FieldNameSite = "_site_s";
        private const string FieldNameStartPublish = "StartPublish";
        private const string FieldNameStopPublish = "StopPublish";

        public DefaultSolrContentRepository(ISolrConfiguration solrConfiguration, ILogger logger)
        {
            _solrConfiguration = solrConfiguration;
            _logger = logger;
        }

        public async Task AddAsync(Guid siteDefinitionId, ContentReference contentReference, IContent newContent = null)
        {
            var contentReferenceId = GetContentReferenceId(contentReference);
            _logger.Log(Level.Information, string.Format("Indexing contentReference {0}", contentReference.ID));
            var languageBranches = GetLanguageBranches(contentReference, newContent).ToList();
            if (!languageBranches.Any()) return;
            var anyContent = languageBranches.First();
            var isCommerceContent = anyContent is CatalogContentBase;
            var documentContent = new Dictionary<string, object>
            {
                {FieldNameId, contentReferenceId},
                {FieldNameSite, (isCommerceContent ? _commerceSiteId : siteDefinitionId).ToString("D")},
                {FieldNameType, GetInheritancHierarchy(anyContent.GetOriginalType()).Select(x => x.FullName)}
            };
            foreach (var languageBranch in languageBranches)
            {
                var languageName = GetLanguage(languageBranch);
                try
                {
                    if (languageName == null) continue;
                    var allValues = new List<object>();
                    var fieldResolver = new EpiSolrFieldResolver(new CultureInfo(languageName));
                    foreach (var propertyInfo in languageBranch.GetOriginalType().GetProperties())
                    {
                        // verify attribute is searchable
                        if (!_mandatoryAttributes.Contains(propertyInfo.Name))
                        {
                            var searchable = propertyInfo.GetAttribute<SearchableAttribute>();
                            if (searchable == null || !searchable.IsSearchable) continue;
                        }
                        // get raw field value and do sanity check
                        var fieldValue = propertyInfo.GetValue(languageBranch);
                        if (fieldValue == null) continue;
                        // get field name and do sanity check
                        var fieldName = fieldResolver.GetFieldName(propertyInfo);
                        if (documentContent.ContainsKey(fieldName))
                        {
                            _logger.Log(Level.Error,
                                string.Format("Duplicate property {0} when adding content reference {1}", fieldName,
                                    contentReferenceId));
                            continue;
                        }
                        // handle different types
                        if (propertyInfo.PropertyType == typeof (XhtmlString))
                        {
                            var xhtmlString = (XhtmlString) fieldValue;
                            fieldValue = HtmlAgilityPackHelper.ConvertHtml(xhtmlString.ToHtmlString());
                        }
                        documentContent.Add(fieldName, fieldValue);
                        allValues.Add(fieldValue.ToString());
                    }
                    var allName = fieldResolver.GetDefaultFieldName();
                    AddOrReplace(documentContent, allName, allValues);
                    // add content link
                    AddOrReplace(documentContent,
                        fieldResolver.GetFieldName("ContentLink", typeof (ContentReference)),
                        languageBranch.ContentLink);
                    // add publishing information
                    var versionable = languageBranch as IVersionable;
                    if (versionable != null)
                    {
                        AddOrReplace(documentContent, fieldResolver.GetFieldName(FieldNameStartPublish, typeof(DateTime)),
                            versionable.StartPublish ?? DateTime.Now);
                        AddOrReplace(documentContent, fieldResolver.GetFieldName(FieldNameStopPublish, typeof(DateTime)),
                            versionable.StopPublish ?? DateTime.MaxValue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(Level.Error, string.Format("Failed to index language branch {0}", languageName), ex);
                }
            }
            var updateRepository = new SolrRepository(_solrConfiguration);
            await updateRepository.AddAsync(documentContent);
        }

        public async Task<EpiSolrSearchResult<TContent>> SearchAsync<TContent>(IQueryable<TContent> query,
            CultureInfo language = null, Guid? siteDefinitionId = null) where TContent : IContent
        {
            var fieldResolver = new EpiSolrFieldResolver(language ?? ContentLanguage.PreferredCulture);
            var queryRepository = new SolrRepository(_solrConfiguration, fieldResolver);
            // add type condition
            var contentType = typeof (TContent).FullName;
            query = query.Filter(x => SolrLiteral.String(FieldNameType) == contentType);
            // add publishstatus condition
            if (typeof (IVersionable).IsAssignableFrom(typeof (TContent)))
            {
                var startName = fieldResolver.GetFieldName(FieldNameStartPublish, typeof (DateTime));
                var endName = fieldResolver.GetFieldName(FieldNameStopPublish, typeof (DateTime));
                // IMPORTANT: round to nearest order, in order to optimize cache!
                // thus immediate expiration of content is done by setting expiration
                // date to more than one hour ago
                query = query.Filter(
                    x =>
                        SolrLiteral.String(startName) == SolrLiteral.String("[* TO NOW/HOUR+1HOUR]") &&
                        SolrLiteral.String(endName) == SolrLiteral.String("[NOW/HOUR TO *]"));
            }
            // filter on current site and all commerce content (zero-guid)
            var siteId = siteDefinitionId.GetValueOrDefault(SiteDefinition.Current.Id).ToString("D");
            var commerceSiteId = _commerceSiteId.ToString("D");
            query =
                query.Filter(
                    x =>
                        SolrLiteral.String(FieldNameSite) == siteId ||
                        SolrLiteral.String(FieldNameSite) == commerceSiteId);
            // do dismax queries in the default field
            query = query.QueryField(fieldResolver.GetDefaultFieldName());
            // get only content links from result
            var solrResult = await queryRepository.SearchAsync<TContent, EpiSolrContentReference>(query, fieldResolver);
            // replace partial results with full results
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var completeDocuments = new List<TContent>();
            foreach (var document in solrResult.Documents)
            {
                ContentReference contentReference;
                TContent completeDocument;
                if (ContentReference.TryParse(document.ContentLink, out contentReference) &&
                    contentRepository.TryGet(contentReference, language, out completeDocument))
                {
                    completeDocuments.Add(completeDocument);
                }
            }
            var finalResult = new EpiSolrSearchResult<TContent>
            {
                Results = completeDocuments,
                SolrQueryResponse = solrResult
            };
            return finalResult;
        }

        public async Task RemoveAsync(ContentReference contentReference)
        {
            var updateRepository = new SolrRepository(_solrConfiguration);
            await updateRepository.RemoveAsync(GetContentReferenceId(contentReference));
        }

        public async Task RemoveAllAsync(Guid siteDefinitionId)
        {
            var updateRepository = new SolrRepository(_solrConfiguration);
            var stringId = siteDefinitionId.ToString("D");
            await
                updateRepository.RemoveAsync<object>(x => SolrLiteral.String(FieldNameSite) == stringId);
        }

        public async Task RemoveAllCommerceContentAsync()
        {
            await RemoveAllAsync(_commerceSiteId);
        }

        private static void AddOrReplace(IDictionary<string, object> dictionary, string key, object value)
        {
            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);
        }

        private static string GetLanguage(IContentData content)
        {
            var commerceContent = content as CatalogContentBase;
            if (commerceContent != null) return commerceContent.Language.Name;
            return content == null || content.Property == null || content.Property.LanguageBranch == null
                ? null
                : content.Property.LanguageBranch;
        }

        private static IEnumerable<IContent> GetLanguageBranches(ContentReference contentReference, IContent newContent)
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var existingLanguageBranches =
                contentRepository.GetLanguageBranches<IContent>(contentReference)
                    .Where(b => GetLanguage(b) != null);
            var newContentLanguage = GetLanguage(newContent);
            var newContentLanguageExists = false;
            foreach (var languageBranch in existingLanguageBranches)
            {
                if (newContent != null &&
                    string.Equals(GetLanguage(languageBranch), newContentLanguage,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    newContentLanguageExists = true;
                    yield return newContent;
                }
                else
                {
                    yield return languageBranch;
                }
            }
            if (!newContentLanguageExists && newContent != null)
            {
                yield return newContent;
            }
        }

        private static IEnumerable<Type> GetInheritancHierarchy(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                yield return current;
            }
            foreach (var @interface in type.GetInterfaces())
            {
                yield return @interface;
            }
        }

        private static string GetContentReferenceId(ContentReference contentReference)
        {
            return contentReference.ProviderName == null
                ? contentReference.ID.ToString(CultureInfo.InvariantCulture)
                : string.Format("{0}_{1}", contentReference.ID, contentReference.ProviderName);
        }
    }
}
