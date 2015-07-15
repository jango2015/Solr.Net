using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Solr.Client.WebService
{
    public class SolrQuery<TDocument> where TDocument : new()
    {
        public readonly SolrClient Client;
        public readonly string Query;
        public string QueryType = "dismax";
        public int Limit = 50;
        public int Offset = 0;
        public readonly List<Expression<Func<TDocument, bool>>> Filters = new List<Expression<Func<TDocument, bool>>>();
        
        public SolrQuery(SolrClient client, string query)
        {
            Client = client;
            Query = query;
        }

        public SolrQuery<TDocument> Filter(Expression<Func<TDocument, bool>> predicate)
        {
            Filters.Add(predicate);
            return this;
        }

        public SolrQuery<TDocument> Skip(int n)
        {
            Offset = n;
            return this;
        }

        public SolrQuery<TDocument> Take(int n)
        {
            Limit = n;
            return this;
        }

        public async Task<SolrQueryResponse<TDocument>> Execute()
        {
            return await Client.Get(this);
        }
    }
}
