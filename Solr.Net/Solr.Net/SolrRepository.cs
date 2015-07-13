using System.Linq;
using Solr.Net.Linq;
using Solr.Net.WebService;

namespace Solr.Net
{
    public class SolrRepository : ISolrRepository
    {
        public SolrRepository(string solrEndpoint)
        {
            
        }
        public SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new()
        {
            return new SolrQuery<TDocument>(new SolrClient(), query);
        }
    }
}