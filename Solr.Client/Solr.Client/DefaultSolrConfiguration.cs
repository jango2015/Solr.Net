namespace Solr.Client
{
    public class DefaultSolrConfiguration : ISolrConfiguration
    {
        public DefaultSolrConfiguration()
        {
        }
        public DefaultSolrConfiguration(string collectionUrl)
        {
            var baseUrl = collectionUrl.TrimEnd('/');
            UpdateUrl = string.Format("{0}/update", baseUrl);
            QueryUrl = string.Format("{0}/query", baseUrl);
        }

        public string UpdateUrl { get; set; }
        public string QueryUrl { get; set; }
    }
}
