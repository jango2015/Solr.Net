using System.Threading.Tasks;
using Solr.Client.WebService;

namespace Solr.Client
{
    public class SolrRepository : ISolrRepository
    {
        private readonly ISolrConfiguration _configruation;

        public SolrRepository(ISolrConfiguration configruation)
        {
            _configruation = configruation;
        }

        public async Task Add<TDocument>(TDocument document)
        {
            await Client.Add(document);
        }

        public SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new()
        {
            return new SolrQuery<TDocument>(Client, query);
        }

        private SolrClient Client
        {
            get
            {
                return new SolrClient(_configruation);
            }
        }
    }
}