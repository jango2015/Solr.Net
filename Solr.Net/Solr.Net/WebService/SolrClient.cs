using System.Collections.Generic;

namespace Solr.Net.WebService
{
    public class SolrClient
    {
        public void Add<TDocument>(TDocument document)
        {
            
        }

        public IEnumerable<TDocument> Get<TDocument>(object query)
        {
            return new TDocument[10];
        }
    }
}
