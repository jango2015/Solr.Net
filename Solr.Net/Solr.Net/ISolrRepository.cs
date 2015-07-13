using System.Threading.Tasks;
using Solr.Net.WebService;

namespace Solr.Net
{
    public interface ISolrRepository
    {
        Task Add(object document);
        SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new();
    }
}
