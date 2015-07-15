using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrDeleteRequest
    {
        public SolrDeleteRequest(object id)
        {
            Id = id;
        }

        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }
    }
}