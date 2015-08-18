using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrDeleteIdRequest : ISolrDeleteRequest
    {
        public SolrDeleteIdRequest(object id)
        {
            Id = id;
        }

        [JsonProperty(PropertyName = "id")]
        public object Id { get; set; }
    }
}