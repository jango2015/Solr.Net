using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;

namespace Solr.EPiServer
{
    public interface ISolrContentRepository
    {
        Task AddAsync(Guid siteDefinitionId, ContentReference contentReference, IContent content = null);

        Task<EpiSolrSearchResult<TContent>> SearchAsync<TContent>(IQueryable<TContent> query, CultureInfo language = null,
            Guid? siteDefinitionId = null) where TContent : IContent;

        Task RemoveAsync(ContentReference contentLink);
        Task RemoveAllAsync(Guid siteDefinitionId);
        Task RemoveAllCommerceContentAsync();
    }
}