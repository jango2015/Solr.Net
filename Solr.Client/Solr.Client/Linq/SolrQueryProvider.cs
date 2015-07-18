using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Solr.Client.Serialization;
using Solr.Client.WebService;

namespace Solr.Client.Linq
{
    public class SolrQueryProvider<TDocument> : IQueryProvider
    {
        private readonly ISolrRepository _solrRepository;

        public SolrQueryProvider(ISolrRepository solrRepository)
        {
            _solrRepository = solrRepository;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<TDocument>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SolrQuery<TElement>(expression, this);
        }

        public object Execute(Expression expression)
        {
            throw new NotSupportedException();
            //return Execute<TDocument>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var request = new SolrExpressionVisitor(_solrRepository.FieldResolver).Translate(expression);
            var result = _solrRepository.Client.Get<TDocument>(request).Result;

            var resultType = typeof (TResult);
            if (typeof (TResult) == typeof (IEnumerable<TDocument>))
            {
                return (TResult) result.Response.Documents;
            }
            if (resultType == typeof (SolrQueryResponse<TDocument>))
            {
                return (TResult) (object) result;
            }

            throw new NotSupportedException();
        }
    }
}
