using System.Collections.Generic;
using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrQueryResponse<TDocument> : SolrResponse
    {
        public ResponseNode Response { get; set; }

        public class ResponseNode
        {
            public int NumFound { get; set; }
            public int Start { get; set; }
            [JsonProperty(PropertyName = "docs")]
            public IEnumerable<TDocument> Documents { get; set; }
        }
    }
}
