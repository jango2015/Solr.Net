using System.Globalization;
using System.Threading.Tasks;
using EPiServer.Core;
using Solr.Client.WebService;

namespace Solr.EPiServer
{
    public interface ISolrContentRepository
    {
        Task Add(ContentReference contentReference, IContent content = null);
        Task<SolrQueryResponse<TContent>> Query<TContent>(SolrQuery<TContent> query, CultureInfo language = null) where TContent : IContent;
        Task Remove(ContentReference contentLink);
    }
}