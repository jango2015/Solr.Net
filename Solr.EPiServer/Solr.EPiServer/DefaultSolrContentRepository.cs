﻿using System.Globalization;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Solr.Client;
using Solr.Client.Serialization;
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
            var updateRepository = new DefaultSolrRepository(_solrConfiguration, new DefaultSolrFieldResolver());
            await updateRepository.Add(contentReference);
        }

        public SolrQuery<TContent> Query<TContent>(string query, CultureInfo language = null) where TContent : IContent, new()
        {
            var fieldResolver = new EpiSolrFieldResolver(language ?? LanguageSelector.AutoDetect(false).Language);
            var queryRepository = new DefaultSolrRepository(_solrConfiguration, fieldResolver);
            return queryRepository.Get<TContent>(query);
        }
    }
}
