namespace Solr.Net.WebService
{
    public class SolrResponse
    {
        public SolrResponseHeader ResponseHeader { get; set; }
        public SolrResponseError Error { get; set; }

        public bool IsSuccessStatusCode
        {
            get { return ResponseHeader != null && ResponseHeader.Status == 0; }
        }
    }
}