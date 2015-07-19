using System;
using System.Linq;
using System.Linq.Expressions;

namespace Solr.Client.Linq
{
    public class SolrQueryProvider : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            
            throw new NotSupportedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SolrQuery<TElement>(expression, this);
        }

        public object Execute(Expression expression)
        {
            throw new NotSupportedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult) Execute(expression);
        }
    }
}
