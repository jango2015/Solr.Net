using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Solr.Client.WebService
{
    public class SolrQuery<TDocument>
    {
        public readonly string Query;
        public string QueryType;
        public int Limit = 50;
        public int Offset = 0;
        public readonly List<Expression<Func<TDocument, bool>>> Filters = new List<Expression<Func<TDocument, bool>>>();
        
        public SolrQuery(string query, string queryType = "dismax")
        {
            Query = query;
            QueryType = queryType;
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
    }
}
