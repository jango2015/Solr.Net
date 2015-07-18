using Newtonsoft.Json;

namespace Solr.Client.WebService
{
    public class SolrResponseError
    {
        public long Code { get; set; }
        [JsonProperty(PropertyName = "Msg")]
        public string Message { get; set; }
        public string Trace { get; set; }

        public string GetDescription()
        {
            return string.Format("{0} {1}: {2}", Code, Message, Trace);
        }
    }
}