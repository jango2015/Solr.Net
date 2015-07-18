using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Solr.Client.Serialization;

namespace Solr.Client.WebService
{
    public class SolrQuery<TDocument>
    {
        internal readonly Expression<Func<TDocument, object>> Query;
        internal string QueryType;
        internal int Limit = 50;
        internal int Offset = 0;
        internal readonly List<Expression<Func<TDocument, object>>> Filters = new List<Expression<Func<TDocument, object>>>();
        internal readonly List<Expression<Func<TDocument, object>>> QueryFields = new List<Expression<Func<TDocument, object>>>();
        internal readonly Dictionary<string, ISolrQueryFacet> Facets = new Dictionary<string, ISolrQueryFacet>();

        public SolrQuery()
        {
            Query = document => SolrLiteral.String("*");
            QueryType = "lucene";
        }

        public SolrQuery(string query, string queryType = "dismax")
        {
            Query = document => SolrLiteral.String(query);
            QueryType = queryType;
        }
        public SolrQuery(Expression<Func<TDocument, object>> query, string queryType = "lucene")
        {
            Query = query;
            QueryType = queryType;
        }

        public SolrQuery<TDocument> Filter(string query)
        {
            Filters.Add(x => SolrLiteral.String(query));
            return this;
        }

        public SolrQuery<TDocument> Filter(Expression<Func<TDocument, object>> predicate)
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

        public SolrQuery<TDocument> QueryField(string field)
        {
            QueryFields.Add(x => SolrLiteral.String(field));
            return this;
        }

        public SolrQuery<TDocument> QueryField(Expression<Func<TDocument, object>> expression)
        {
            QueryFields.Add(expression);
            return this;
        }

        public SolrQuery<TDocument> Facet(string name, ISolrQueryFacet facet)
        {
            Facets.Add(name, facet);
            return this;
        }
    }
}
