using System.Collections.Generic;
using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public interface ISolrQueryFacet
    {
    }

    public abstract class SolrQueryFacet : ISolrQueryFacet
    {
        private readonly Dictionary<string, ISolrQueryFacet> _facets = new Dictionary<string, ISolrQueryFacet>();

        [JsonProperty(PropertyName = "facet")]
        public Dictionary<string, ISolrQueryFacet> Facets
        {
            get { return _facets; }
        }
    }

    public class SolrQueryTermsFacet : SolrQueryFacet
    {
        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get { return "terms"; }
        }

        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        [JsonProperty(PropertyName = "offset")]
        public int? Offset { get; set; }

        [JsonProperty(PropertyName = "limit")]
        public int? Limit { get; set; }

        [JsonProperty(PropertyName = "mincount")]
        public int? MinCount { get; set; }

        [JsonProperty(PropertyName = "numBuckets")]
        public bool? NumBuckets { get; set; }

        [JsonProperty(PropertyName = "allBuckets")]
        public bool? AllBuckets { get; set; }

        [JsonProperty(PropertyName = "prefix")]
        public string Prefix { get; set; }


    }

    public class SolrQueryRangeFacet<TRange> : SolrQueryFacet
    {
        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get { return "range"; }
        }

        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        [JsonProperty(PropertyName = "mincount")]
        public int? MinCount { get; set; }

        [JsonProperty(PropertyName = "start")]
        public TRange Start { get; set; }

        [JsonProperty(PropertyName = "end")]
        public TRange End { get; set; }

        [JsonProperty(PropertyName = "gap")]
        public string Gap { get; set; }

        [JsonProperty(PropertyName = "hardend")]
        public bool? Hardend { get; set; }

        [JsonProperty(PropertyName = "other")]
        public string Other { get; set; }

        [JsonProperty(PropertyName = "include")]
        public string Include { get; set; }

    }
}
