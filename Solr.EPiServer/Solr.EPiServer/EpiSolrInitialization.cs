using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Solr.EPiServer
{
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [InitializableModule]
    public class EpiSolrInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.DeletedContent += ContentEventsOnDeletedContent;
            contentEvents.DeletedContentLanguage += ContentEventsOnPublishedContent;
            contentEvents.DeletedContentVersion += ContentEventsOnPublishedContent;
            contentEvents.MovedContent += ContentEventsOnPublishedContent;
            contentEvents.PublishedContent += ContentEventsOnPublishedContent;
        }

        private static async void ContentEventsOnDeletedContent(object sender, DeleteContentEventArgs deleteContentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            await solrContentRepository.Remove(deleteContentEventArgs.ContentLink);
        }

        private static async void ContentEventsOnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            if (ContentIsInvalid(contentEventArgs))
            {
                // remove
                await solrContentRepository.Remove(contentEventArgs.ContentLink);
            }
            else
            {
                // update index at new url
                await solrContentRepository.Add(SiteDefinition.Current.Id, contentEventArgs.ContentLink, contentEventArgs.Content);
            }
        }

        private static bool ContentIsInvalid(ContentEventArgs contentEventArgs)
        {
            // ignore root page, if it is not the start page
            if (contentEventArgs.ContentLink.Equals(ContentReference.RootPage, true) && !ContentReference.RootPage.Equals(ContentReference.StartPage, true)) return true;
            // ignore items in the waste basket
            return contentEventArgs.TargetLink == ContentReference.WasteBasket;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.PublishedContent -= ContentEventsOnPublishedContent;
            contentEvents.MovedContent -= ContentEventsOnPublishedContent;
        }
    }
}
