using Solr.Net.WebService;

namespace Solr.Net
{
    public interface ISolrRepository
    {
        void Add(object document);
        SolrQuery<TDocument> Get<TDocument>(string query) where TDocument : new();
    }
}
