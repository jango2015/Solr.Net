using EPiServer.ServiceLocation;
using Solr.Client;

namespace Solr.EPiServer
{
    [ServiceConfiguration(ServiceType = typeof(ISolrConfiguration), Lifecycle = ServiceInstanceScope.Singleton)]
    public class EpiSolrConfiguration : DefaultSolrConfiguration
    {
    }
}
