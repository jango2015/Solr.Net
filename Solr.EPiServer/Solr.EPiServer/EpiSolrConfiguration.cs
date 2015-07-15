using EPiServer.ServiceLocation;
using Solr.Client;
using Solr.Client.Serialization;

namespace Solr.EPiServer
{
    [ServiceConfiguration(ServiceType = typeof(ISolrConfiguration), Lifecycle = ServiceInstanceScope.Singleton)]
    class EpiSolrConfiguration : ISolrConfiguration
    {
        public string UpdateUrl { get; private set; }
        public string QueryUrl { get; private set; }
        public ISolrFieldResolver FieldResolver { get; private set; }
    }
}
