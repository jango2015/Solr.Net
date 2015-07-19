using System.Collections.Generic;
using Solr.Client;

namespace Solr.EPiServer
{
    public class EpiSolrSearchResult<T>
    {
        public IEnumerable<T> Results { get; set; }
        public SolrSearchResult<EpiSolrContentReference> SolrQueryResponse { get; set; }
    }
}
