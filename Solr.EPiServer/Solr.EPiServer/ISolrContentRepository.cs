using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;

namespace Solr.EPiServer
{
    public interface ISolrContentRepository
    {
        Task Add(Guid siteDefinitionId, ContentReference contentReference, IContent content = null);

        Task<EpiSolrSearchResult<TContent>> Search<TContent>(IQueryable<TContent> query, CultureInfo language = null,
            Guid? siteDefinitionId = null) where TContent : IContent;

        Task Remove(ContentReference contentLink);
    }
}