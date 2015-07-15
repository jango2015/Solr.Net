using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrUpdateRequest
    {
        [JsonProperty(PropertyName = "add", NullValueHandling = NullValueHandling.Ignore)]
        public SolrAddRequest Add { get; set; }

        [JsonProperty(PropertyName = "delete", NullValueHandling = NullValueHandling.Ignore)]
        public SolrDeleteRequest Remove { get; set; }

        [JsonProperty(PropertyName = "commit", NullValueHandling = NullValueHandling.Ignore)]
        public object Commit { get; set; }
    }
}
