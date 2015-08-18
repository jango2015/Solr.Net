using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrDeleteQueryRequest : ISolrDeleteRequest
    {
        public SolrDeleteQueryRequest(string query)
        {
            Query = query;
        }

        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }
    }
}