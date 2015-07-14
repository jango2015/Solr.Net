using System.Threading.Tasks;
using Solr.Net.WebService;

namespace Solr.Net
{
    public interface ISolrRepository
    {
        Task Add<TDocument>(TDocument document);
        SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new();
    }
}
