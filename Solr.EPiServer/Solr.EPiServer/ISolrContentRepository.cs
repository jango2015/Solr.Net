using System.Globalization;
using System.Threading.Tasks;
using EPiServer.Core;
using Solr.Client.WebService;

namespace Solr.EPiServer
{
    public interface ISolrContentRepository
    {
        Task Add(ContentReference contentReference, IContent content = null);
        SolrQuery<TContent> Query<TContent>(string query, CultureInfo language = null) where TContent : IContent, new();
        Task Remove(ContentReference contentLink);
    }
}