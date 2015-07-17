using System.Collections.Generic;
using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrQueryRequest
    {
        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }
        [JsonProperty(PropertyName = "filter")]
        public IEnumerable<string> Filters { get; set; }
        [JsonProperty(PropertyName = "offset")]
        public int Offset { get; set; }
        [JsonProperty(PropertyName = "limit")]
        public int Limit { get; set; }
    }
}
