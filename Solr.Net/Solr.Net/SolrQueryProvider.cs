using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Solr.Net.Helpers;

namespace Solr.Net
{
    internal class SolrQueryProvider : IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeHelper.GetElementType(expression.Type);
            var queryableType = typeof (SolrQueryable<>).MakeGenericType(elementType);
            var queryable = Activator.CreateInstance(queryableType, new object[] {this, expression});
            return (IQueryable) queryable;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SolrQueryable<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return SolrQueryContext.Execute(expression, false);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var isEnumerable = typeof (IEnumerable<>).IsAssignableFrom(typeof (TResult));
            return (TResult) SolrQueryContext.Execute(expression, isEnumerable);
        }
    }
}