using System.Threading.Tasks;
using Solr.Client.WebService;

namespace Solr.Client
{
    public class DefaultSolrRepository : ISolrRepository
    {
        private readonly ISolrConfiguration _configruation;

        public DefaultSolrRepository(ISolrConfiguration configruation)
        {
            _configruation = configruation;
        }

        public virtual async Task Add<TDocument>(TDocument document)
        {
            await Client.Add(document);
        }

        public virtual SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new()
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