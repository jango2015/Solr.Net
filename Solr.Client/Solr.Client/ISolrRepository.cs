using System.Linq;
using System.Threading.Tasks;
using Solr.Client.Serialization;
using Solr.Client.WebService;

namespace Solr.Client
{
    public interface ISolrRepository
    {
        Task Add<TDocument>(TDocument document);
        IQueryable<TDocument> Search<TDocument>();
        Task Remove(object id);
        SolrClient Client { get; }
        ISolrFieldResolver FieldResolver { get; }
    }
}
