using EPiServer.ServiceLocation;
using Solr.Client;

namespace Solr.EPiServer
{
    [ServiceConfiguration(ServiceType = typeof(ISolrConfiguration), Lifecycle = ServiceInstanceScope.Singleton)]
    class DefaultSolrConfiguration : ISolrConfiguration
    {
        public string UpdateUrl
        {
            get { return "http://localhost:8983/solr/test/update"; }
        }

        public string QueryUrl
        {
            get { return "http://localhost:8983/solr/test/query"; }
        }
    }
}
