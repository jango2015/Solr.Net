using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Solr.EPiServer
{
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [InitializableModule]
    public class EpiSolrInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishedContent += ContentEventsOnPublishedContent;
            contentEvents.MovedContent += ContentEventsOnMovedContent;
        }

        private static async void ContentEventsOnMovedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            if (contentEventArgs.TargetLink == ContentReference.WasteBasket)
            {
                // remove
                await solrContentRepository.Remove(contentEventArgs.ContentLink);
            }
            else
            {
                // update index at new url
                await solrContentRepository.Add(contentEventArgs.ContentLink, contentEventArgs.Content);
            }
        }

        private static async void ContentEventsOnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            await solrContentRepository.Add(contentEventArgs.ContentLink, contentEventArgs.Content);
        }

        public void Uninitialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishedContent -= ContentEventsOnPublishedContent;
        }
    }
}
