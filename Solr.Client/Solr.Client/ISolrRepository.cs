using System.Threading.Tasks;
using Solr.Client.WebService;

namespace Solr.Client
{
    public interface ISolrRepository
    {
        Task Add<TDocument>(TDocument document);
        Task<SolrQueryResponse<TDocument>> Get<TDocument>(SolrQuery<TDocument> query) where TDocument : new();
        Task Remove(object id);
    }
}
