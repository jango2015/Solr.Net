using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Solr.Client.WebService
{
    public class SolrQueryResponse<TDocument> : SolrResponse
    {
        public ResponseNode Response { get; set; }
        [JsonProperty(PropertyName = "facets")]
        internal JToken Facets { get; set; }

        public class ResponseNode
        {
            public long NumFound { get; set; }
            public long Start { get; set; }
            [JsonProperty(PropertyName = "docs")]
            public IEnumerable<TDocument> Documents { get; set; }
        }

        public FacetNode GetFacets()
        {
            var result = Facets.ToObject<FacetNode>();
            var facets = Facets.ToObject<IDictionary<string, object>>();
            foreach (var facet in facets)
            {
                var key = facet.Key;
                var jobj = facet.Value as JObject;
                if (jobj != null)
                {
                    var buckets = jobj["buckets"];
                    if (buckets != null)
                    {
                        var value = buckets.ToObject<IEnumerable<FacetNode>>();
                        if (result.Terms.ContainsKey(key)) result.Terms[key] = value;
                        else result.Terms.Add(key, value);
                    }
                }
                else
                {
                    decimal d;
                    if (decimal.TryParse(facet.Value.ToString(), out d))
                    {
                        if (result.Statistics.ContainsKey(key)) result.Statistics[key] = d;
                        else result.Statistics.Add(key, d);
                    }
                }
            }
            return result;
        }

        public class FacetNode
        {
            public long Count { get; set; }
            [JsonProperty(PropertyName = "val")]
            public string Value { get; set; }
            public IDictionary<string, IEnumerable<FacetNode>> Terms = new Dictionary<string, IEnumerable<FacetNode>>();
            public IDictionary<string, decimal> Statistics = new Dictionary<string, decimal>();
            internal JToken Facets { get; set; }
        }

    }
}
