using Solr.Client.Serialization;

namespace Solr.Client
{
    public interface ISolrConfiguration
    {
        string UpdateUrl { get; }
        string QueryUrl { get; }
        IFieldResolver FieldResolver { get; }
    }

    public class DefaultSolrConfiguration : ISolrConfiguration
    {
        public DefaultSolrConfiguration(string collectionUrl)
        {
            var baseUrl = collectionUrl.TrimEnd('/');
            UpdateUrl = string.Format("{0}/update", baseUrl);
            QueryUrl = string.Format("{0}/query", baseUrl);
            FieldResolver = new DefaultFieldResolver();
        }

        public virtual string UpdateUrl { get; private set; }
        public virtual string QueryUrl { get; private set; }
        public IFieldResolver FieldResolver { get; set; }
    }
}
