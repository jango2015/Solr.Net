using System.Linq;

namespace Solr.Net
{
    public class SolrRepository : ISolrRepository
    {
        public SolrRepository(string queryUrl)
        {
            
        }
        public IQueryable<T> Get<T>() where T : new()
        {
            return new SolrQueryable<T>();
        }
    }
}