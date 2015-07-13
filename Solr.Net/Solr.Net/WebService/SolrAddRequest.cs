using System.Collections.Generic;
using Newtonsoft.Json;

namespace Solr.Net.WebService
{
    public class SolrAddRequest
    {
        [JsonProperty(PropertyName = "Add")]
        public readonly object Document;
        public int Boost = 1;
        public int CommitWithin = 1000;
        public bool Overwrite = true;
        public SolrAddRequest(object document)
        {
            Document = document;
        }
    }
}