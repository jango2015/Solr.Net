using System.Threading.Tasks;
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

        private static void ContentEventsOnDeletedContent(object sender, DeleteContentEventArgs deleteContentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            StartAndWait(solrContentRepository.RemoveAsync(deleteContentEventArgs.ContentLink));
        }

        private static void ContentEventsOnPublishedContent(object sender, ContentEventArgs contentEventArgs)
        {
            var solrContentRepository = ServiceLocator.Current.GetInstance<ISolrContentRepository>();
            if (ContentIsInvalid(contentEventArgs))
            {
                // remove
                StartAndWait(solrContentRepository.RemoveAsync(contentEventArgs.ContentLink));
            }
            else
            {
                // update index at new url
                StartAndWait(solrContentRepository.AddAsync(SiteDefinition.Current.Id, contentEventArgs.ContentLink, contentEventArgs.Content));
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

        private static void StartAndWait(Task task)
        {
            task.ConfigureAwait(false);
        }
    }
}
