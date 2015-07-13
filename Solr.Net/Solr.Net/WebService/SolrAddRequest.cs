using Newtonsoft.Json;

namespace Solr.Net.WebService
{
    public class SolrAddRequest
    {
        [JsonProperty(PropertyName = "doc")]
        public object Document;
        [JsonProperty(PropertyName = "boost")]
        public int Boost = 1;
        [JsonProperty(PropertyName = "commitWithin")]
        public int CommitWithin = 1000;
        [JsonProperty(PropertyName = "overwrite")]
        public bool Overwrite = true;

        public SolrAddRequest(object document)
        {
            Document = document;
        }
    }
}