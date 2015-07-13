using System.Collections.Generic;

namespace Solr.Net.WebService
{
    public class SolrQueryResponse<TDocument> : SolrResponse
    {
        public IEnumerable<TDocument> Documents { get; set; }
    }
}