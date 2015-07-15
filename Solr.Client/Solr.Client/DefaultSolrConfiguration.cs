using Solr.Client.Serialization;

namespace Solr.Client
{
    public class DefaultSolrConfiguration : ISolrConfiguration
    {
        public DefaultSolrConfiguration(string collectionUrl)
        {
            var baseUrl = collectionUrl.TrimEnd('/');
            UpdateUrl = string.Format("{0}/update", baseUrl);
            QueryUrl = string.Format("{0}/query", baseUrl);
            FieldResolver = new DefaultSolrFieldResolver();
        }

        public virtual string UpdateUrl { get; private set; }
        public virtual string QueryUrl { get; private set; }
        public ISolrFieldResolver FieldResolver { get; set; }
    }
}