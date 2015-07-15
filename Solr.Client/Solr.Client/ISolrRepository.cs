using System.Threading.Tasks;
using Solr.Client.WebService;

namespace Solr.Client
{
    public interface ISolrRepository
    {
        Task Add<TDocument>(TDocument document);
        SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new();
        Task Remove(object id);
    }
}
