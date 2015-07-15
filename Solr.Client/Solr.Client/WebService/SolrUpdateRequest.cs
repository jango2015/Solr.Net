using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrUpdateRequest
    {
        [JsonProperty(PropertyName = "add")]
        public SolrAddRequest Add { get; set; }
        
    }
}