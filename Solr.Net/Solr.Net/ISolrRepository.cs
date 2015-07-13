using System.Linq;
using Solr.Net.WebService;

namespace Solr.Net
{
    public interface ISolrRepository
    {
        SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new();
    }
}
