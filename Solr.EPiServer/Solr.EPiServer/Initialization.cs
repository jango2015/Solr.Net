using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Solr.EPiServer
{
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [InitializableModule]
    public class Initialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishedContent += contentEvents_PublishedContent;
        }

        private void contentEvents_PublishedContent(object sender, ContentEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        public void Uninitialize(InitializationEngine context)
        {
            throw new System.NotImplementedException();
        }
    }
}
