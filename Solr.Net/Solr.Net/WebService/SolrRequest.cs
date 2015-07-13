using System.Collections.Generic;

namespace Solr.Net.WebService
{
    public class SolrRequest
    {
        public string Query { get; set; }
        public IEnumerable<string> Filters { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
    }
}