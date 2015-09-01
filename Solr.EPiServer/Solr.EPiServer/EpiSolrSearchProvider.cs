using System;
using System.Collections.Generic;
using EPiServer.Logging;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Search;

namespace Solr.EPiServer
{
    public class EpiSolrSearchProvider : SearchProvider
    {
        private const string CommerceProviderName = "CatalogContent";

        private readonly ISolrContentRepository _solrRepository;
        private readonly ILogger _logger;
        private readonly HashSet<int> _history;

        public EpiSolrSearchProvider()
        {
            _logger = ServiceLocator.Current.GetInstance<ILogger>();
            _solrRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            _history = new HashSet<int>();
        }

        public override ISearchResults Search(string applicationName, ISearchCriteria criteria)
        {
            throw new NotSupportedException("Use ISolrContentRepository instead.");
        }

        public override void Index(string applicationName, string scope, ISearchDocument document)
        {
            try
            {
                var contentId = int.Parse((string)document["_id"].Value);
                // dont index same twice, multiple languages are handled in repository.add
                if (_history.Contains(contentId)) return;
                _history.Add(contentId);
                // now index
                var siteDefinitionId = Guid.Empty; // commerce content is not site specific
                var contentReference = new ContentReference(contentId, CommerceProviderName);
                _solrRepository.AddAsync(siteDefinitionId, contentReference).Wait();
            }
            catch (Exception ex)
            {
                _logger.Log(Level.Error, "Failed to index a commerce document", ex);
            }
        }

        public override int Remove(string applicationName, string scope, string key, string value)
        {
            if (key == "_id")
            {
                var contentId = int.Parse(value);
                _solrRepository.RemoveAsync(new ContentReference(contentId, CommerceProviderName)).Wait();
                _history.Remove(contentId);
            }
            else
            {
                var errorMessage = string.Format("Cannot remove content by key: {0}. Value is: {1}", key, value);
                _logger.Log(Level.Error, errorMessage);
                //throw new NotSupportedException(errorMessage);
            }
            return 1;
        }

        public override void RemoveAll(string applicationName, string scope)
        {
            _solrRepository.RemoveAllCommerceContentAsync().Wait();
        }

        public override void Close(string applicationName, string scope)
        {
            // whipe history
            _history.Clear();
        }

        public override void Commit(string applicationName)
        {
            // whipe history
            _history.Clear();
        }

        public override string QueryBuilderType
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
