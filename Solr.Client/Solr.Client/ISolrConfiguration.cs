using Solr.Client.Serialization;

namespace Solr.Client
{
    public interface ISolrConfiguration
    {
        string UpdateUrl { get; }
        string QueryUrl { get; }
    }
}
