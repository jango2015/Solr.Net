using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Solr.Client;
using Solr.Client.Linq;
using Solr.EPiServer.Helpers;

namespace Solr.EPiServer
{
    [ServiceConfiguration(ServiceType = typeof(ISolrContentRepository), Lifecycle = ServiceInstanceScope.HttpContext)]
    public class DefaultSolrContentRepository : ISolrContentRepository
    {
        private readonly ISolrConfiguration _solrConfiguration;
        private readonly ILogger _logger;
        private readonly HashSet<string> _mandatoryAttributes = new HashSet<string> { "PageName" };

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

        public async Task Add(Guid siteDefinitionId, ContentReference contentReference, IContent newContent = null)
        {
            _logger.Log(Level.Information, string.Format("Indexing contentReference {0}", contentReference.ID));
            var documentContent = new Dictionary<string, object>
            {
                {FieldNameId, contentReference.ID},
                {FieldNameSite, siteDefinitionId.ToString("D")},
                {FieldNameType, GetInheritancHierarchy(newContent.GetOriginalType()).Select(x => x.FullName)}
            };
            foreach (var languageBranch in GetLanguageBranches(contentReference, newContent))
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
                                    contentReference.ID));
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
            await updateRepository.Add(documentContent);
        }

        private static void AddOrReplace(IDictionary<string, object> dictionary, string key, object value)
        {
            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);
        }

        private static string GetLanguage(IContentData content)
        {
            return content == null || content.Property == null || content.Property.LanguageBranch == null
                ? null
                : content.Property.LanguageBranch;
        }

        public async Task<EpiSolrSearchResult<TContent>> Search<TContent>(IQueryable<TContent> query,
            CultureInfo language = null, Guid? siteDefinitionId = null) where TContent : IContent
        {
            var fieldResolver = new EpiSolrFieldResolver(language ?? LanguageSelector.AutoDetect(false).Language);
            var queryRepository = new SolrRepository(_solrConfiguration, fieldResolver);
            // add type condition
            var contentType = typeof (TContent).FullName;
            query.Filter(x => SolrLiteral.String(FieldNameType) == contentType);
            // add publishstatus condition
            if (typeof (IVersionable).IsAssignableFrom(typeof (TContent)))
            {
                var startName = fieldResolver.GetFieldName(FieldNameStartPublish, typeof (DateTime));
                var endName = fieldResolver.GetFieldName(FieldNameStopPublish, typeof (DateTime));
                // IMPORTANT: round to nearest order, in order to optimize cache!
                // thus immediate expiration of content is done by setting expiration
                // date to more than one hour ago
                query.Filter(
                    x =>
                        SolrLiteral.String(startName) == SolrLiteral.String("[* TO NOW/HOUR+1HOUR]") &&
                        SolrLiteral.String(endName) == SolrLiteral.String("[NOW/HOUR TO *]"));
            }
            // filter on site
            var siteId = siteDefinitionId.GetValueOrDefault(SiteDefinition.Current.Id).ToString("D");
            query.Filter(x => SolrLiteral.String(FieldNameSite) == siteId);
            // do dismax queries in the default field
            query.QueryField(fieldResolver.GetDefaultFieldName());
            // get only content links from result
            var solrResult = await queryRepository.Search<TContent, EpiSolrContentReference>(query);
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

        public async Task Remove(ContentReference contentReference)
        {
            var updateRepository = new SolrRepository(_solrConfiguration);
            await updateRepository.Remove(contentReference.ID);
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
                if (string.Equals(GetLanguage(languageBranch), newContentLanguage,
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
            if (!newContentLanguageExists)
            {
                yield return newContent;
            }
        }

        public static IEnumerable<Type> GetInheritancHierarchy(Type type)
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
    }
}
