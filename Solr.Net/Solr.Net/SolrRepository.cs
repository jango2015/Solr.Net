using System.Threading.Tasks;
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

        public async Task Add<TDocument>(TDocument document)
        {
            await _client.Add(document);
        }

        public SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new()
        {
            return new SolrQuery<TDocument>(_client, query);
        }
    }
}