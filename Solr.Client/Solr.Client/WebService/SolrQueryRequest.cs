using System.Collections.Generic;
using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrQueryRequest
    {
        [JsonProperty(PropertyName = "json")]
        public JsonNode Json = new JsonNode();

        [JsonProperty(PropertyName = "deftype")]
        public string QueryType
        {
            get { return _queryType; }
            set { _queryType = value; }
        }

        [JsonProperty(PropertyName = "sort", NullValueHandling = NullValueHandling.Ignore)]
        public string Sort { get; set; }

        [JsonProperty(PropertyName = "qf")]
        public List<string> QueryFields = new List<string>();

        private string _queryType = "lucene";

        public class JsonNode
        {
            private string _query = "*";
            private readonly List<string> _filters = new List<string>();
            private readonly Dictionary<string, ISolrQueryFacet> _facets = new Dictionary<string, ISolrQueryFacet>();

            [JsonProperty(PropertyName = "query")]
            public string Query
            {
                get { return _query; }
                set { _query = value; }
            }

            [JsonProperty(PropertyName = "filter")]
            public List<string> Filters
            {
                get { return _filters; }
            }

            [JsonProperty(PropertyName = "offset")]
            public int? Offset { get; set; }
            [JsonProperty(PropertyName = "limit")]
            public int? Limit { get; set; }

            [JsonProperty(PropertyName = "facet")]
            public Dictionary<string, ISolrQueryFacet> Facets
            {
                get { return _facets; }
            }
        }
    }
}
