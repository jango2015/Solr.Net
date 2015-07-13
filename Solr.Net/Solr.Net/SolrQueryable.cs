using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Solr.Net
{
    class SolrQueryable<T> : IQueryable<T>
    {
        public SolrQueryable()
        {
            Expression = Expression.Constant(this);
            Provider = new SolrQueryProvider();
        }

        public SolrQueryable(IQueryProvider queryProvider, Expression expression)
        {
            Expression = expression;
            Provider = queryProvider;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof (T); }
        }

        public Expression Expression { get; private set; }
        public IQueryProvider Provider { get; private set; }
    }
}
