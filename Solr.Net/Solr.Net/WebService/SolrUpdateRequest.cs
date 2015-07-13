using Newtonsoft.Json;

namespace Solr.Net.WebService
{
    public class SolrUpdateRequest
    {
        [JsonProperty(PropertyName = "add")]
        public SolrAddRequest Add { get; set; }
        
    }
}