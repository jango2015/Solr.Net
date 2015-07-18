using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Solr.Client.Linq
{
    public class SolrQuery<TDocument> : IOrderedQueryable<TDocument>
    {

        public SolrQuery(Expression expression, IQueryProvider provider)
        {
            Expression = expression;
            Provider = provider;
        }

        public SolrQuery(ISolrRepository solrRepository)
        {
            Expression = Expression.Constant(this);
            Provider = new SolrQueryProvider<TDocument>(solrRepository);
        }

        public IEnumerator<TDocument> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TDocument>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get; private set; }
        public Type ElementType { get { return typeof(TDocument); } }
        public IQueryProvider Provider { get; private set; }
    }
}
