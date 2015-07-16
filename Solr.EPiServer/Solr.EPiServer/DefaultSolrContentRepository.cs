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
using Solr.Client;
using Solr.Client.WebService;
using Solr.EPiServer.Helpers;

namespace Solr.EPiServer
{
    [ServiceConfiguration(ServiceType = typeof(ISolrContentRepository), Lifecycle = ServiceInstanceScope.HttpContext)]
    public class DefaultSolrContentRepository : ISolrContentRepository
    {
        private readonly ISolrConfiguration _solrConfiguration;
        private readonly ILogger _logger;
        private readonly HashSet<string> _mandatoryAttributes = new HashSet<string> { "PageName" };

        public DefaultSolrContentRepository(ISolrConfiguration solrConfiguration, ILogger logger)
        {
            _solrConfiguration = solrConfiguration;
            _logger = logger;
        }

        public async Task Add(ContentReference contentReference, IContent newContent = null)
        {
            _logger.Log(Level.Information, string.Format("Indexing contentReference {0}", contentReference.ID));
            var documentContent = new Dictionary<string, object> {{"id", contentReference.ID}};
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
                            fieldValue = HtmlHelper.ConvertHtml(xhtmlString.ToHtmlString());
                        }
                        documentContent.Add(fieldName, fieldValue);
                        allValues.Add(fieldValue.ToString());
                    }
                    var allName = fieldResolver.GetDefaultFieldName();
                    AddOrReplace(documentContent, allName, allValues);
                    // add publishing information
                    var versionable = languageBranch as IVersionable;
                    if (versionable != null)
                    {
                        AddOrReplace(documentContent, fieldResolver.GetFieldName("StartPublish", typeof(DateTime)),
                            versionable.StartPublish ?? DateTime.Now);
                        AddOrReplace(documentContent, fieldResolver.GetFieldName("StopPublish", typeof(DateTime)),
                            versionable.StopPublish ?? DateTime.MaxValue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(Level.Error, string.Format("Failed to index language branch {0}", languageName), ex);
                }
            }
            var updateRepository = new DefaultSolrRepository(_solrConfiguration);
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

        public SolrQuery<TContent> Query<TContent>(string query, CultureInfo language = null) where TContent : IContent, new()
        {
            var fieldResolver = new EpiSolrFieldResolver(language ?? LanguageSelector.AutoDetect(false).Language);
            var queryRepository = new DefaultSolrRepository(_solrConfiguration, fieldResolver);
            return queryRepository.Get<TContent>(query);
        }

        public async Task Remove(ContentReference contentReference)
        {
            var updateRepository = new DefaultSolrRepository(_solrConfiguration);
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
    }
}
