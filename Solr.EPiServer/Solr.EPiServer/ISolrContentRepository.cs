using System;
using System.Globalization;
using System.Threading.Tasks;
using EPiServer.Core;
using Solr.Client.WebService;

namespace Solr.EPiServer
{
    public interface ISolrContentRepository
    {
        Task Add(Guid siteDefinitionId, ContentReference contentReference, IContent content = null);
        Task<EpiSolrSearchResult<TContent>> Query<TContent>(SolrQuery<TContent> query, CultureInfo language = null, Guid? siteDefinitionId = null) where TContent : IContent;
        Task Remove(ContentReference contentLink);
    }
}