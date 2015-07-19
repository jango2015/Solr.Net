using System.Linq;

namespace Solr.Client.Linq
{
    public interface ISolrFacetQuery<out TDocument> : IQueryable<TDocument>
    {
    }
    public class SolrRangeFacetQuery<TDocument, TRange> : SolrQuery<TDocument>
    {
    }
    public class SolrTermsFacetQuery<TDocument> : SolrQuery<TDocument>
    {
    }
}
