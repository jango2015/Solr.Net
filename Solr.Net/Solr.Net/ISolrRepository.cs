using System.Linq;

namespace Solr.Net
{
    public interface ISolrRepository
    {
        IQueryable<T> Get<T>() where T : new();
    }
}
