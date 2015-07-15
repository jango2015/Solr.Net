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
            contentEvents.PublishedContent += ContentEventsOnPublishedContent;
            contentEvents.RejectedContent += ContentEventsOnRejectedContent;
        }

        private static async void ContentEventsOnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            await solrContentRepository.Add(contentEventArgs.ContentLink);
        }

        private static async void ContentEventsOnRejectedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            await solrContentRepository.Remove(contentEventArgs.ContentLink);
        }

        public void Uninitialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishedContent -= ContentEventsOnPublishedContent;
            contentEvents.RejectedContent -= ContentEventsOnRejectedContent;
        }
    }
}
