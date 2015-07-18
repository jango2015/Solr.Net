using System.Linq;
using System.Threading.Tasks;
using Solr.Client.Linq;
using Solr.Client.Serialization;
using Solr.Client.WebService;

namespace Solr.Client
{
    public class DefaultSolrRepository : ISolrRepository
    {
        private readonly ISolrConfiguration _configruation;

        public DefaultSolrRepository(ISolrConfiguration configruation, ISolrFieldResolver fieldResolver = null)
        {
            _configruation = configruation;
            FieldResolver = fieldResolver ?? new DefaultSolrFieldResolver();
        }

        public virtual async Task Add<TDocument>(TDocument document)
        {
            await Client.Add(document);
        }

        public virtual IQueryable<TDocument> Search<TDocument>()
        {
            return new SolrQuery<TDocument>(this);
        }

        public SolrClient Client
        {
            get
            {
                return new SolrClient(_configruation, FieldResolver);
            }
        }

        public async Task Remove(object id)
        {
            await Client.Remove(id);
        }

        public ISolrFieldResolver FieldResolver { get; private set; }
    }
}