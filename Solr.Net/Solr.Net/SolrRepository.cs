using Solr.Net.WebService;

namespace Solr.Net
{
    public class SolrRepository : ISolrRepository
    {
        private readonly SolrClient _client;

        public SolrRepository(SolrClient client)
        {
            _client = client;
        }

        public void Add(object document)
        {
            _client.Add(document);
        }

        public SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new()
        {
            return new SolrQuery<TDocument>(_client, query);
        }
    }
}