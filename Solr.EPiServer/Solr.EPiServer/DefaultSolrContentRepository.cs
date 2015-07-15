using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Solr.Client;
using Solr.Client.WebService;

namespace Solr.EPiServer
{
    [ServiceConfiguration(ServiceType = typeof(ISolrContentRepository), Lifecycle = ServiceInstanceScope.HttpContext)]
    public class DefaultSolrContentRepository : ISolrContentRepository
    {
        private readonly ISolrConfiguration _solrConfiguration;

        public DefaultSolrContentRepository(ISolrConfiguration solrConfiguration)
        {
            _solrConfiguration = solrConfiguration;
        }

        public async Task Add(ContentReference contentReference)
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var languageBranches = contentRepository.GetLanguageBranches<IContent>(contentReference);
            var documentContent = new Dictionary<string, object> {{"id", contentReference.ID}};
            foreach (var languageBranch in languageBranches)
            {
                var fieldResolver = new EpiSolrFieldResolver(new CultureInfo(languageBranch.Property.LanguageBranch));
                foreach (var propertyInfo in languageBranch.GetType().GetProperties())
                {
                    if (propertyInfo.PropertyType == typeof (string))
                    {
                        documentContent.Add(
                            fieldResolver.GetFieldName(propertyInfo),
                            propertyInfo.GetValue(languageBranch));
                    }
                }
            }
            var updateRepository = new DefaultSolrRepository(_solrConfiguration);
            await updateRepository.Add(documentContent);
        }

        public SolrQuery<TContent> Query<TContent>(string query, CultureInfo language = null) where TContent : IContent, new()
        {
            var fieldResolver = new EpiSolrFieldResolver(language ?? LanguageSelector.AutoDetect(false).Language);
            var queryRepository = new DefaultSolrRepository(_solrConfiguration, fieldResolver);
            return queryRepository.Get<TContent>(query);
        }

        public async Task Remove(ContentReference contentReference)
        {
            throw new NotImplementedException();
            //var updateRepository = new DefaultSolrRepository(_solrConfiguration);
            //await updateRepository.Remove(contentReference.ID);
        }
    }
}
