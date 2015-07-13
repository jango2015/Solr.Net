using Newtonsoft.Json;

namespace Solr.Net.WebService
{
    public class SolrResponseError
    {
        public int Code { get; set; }
        [JsonProperty(PropertyName = "Msg")]
        public string Message { get; set; }
        public string Trace { get; set; }

        public string GetDescription()
        {
            return string.Format("{0} {1}: {2}", Code, Message, Trace);
        }
    }
}