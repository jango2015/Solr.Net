using System;
using System.Linq.Expressions;
using Solr.Client.Serialization;
using Solr.Client.WebService;

namespace Solr.Client.Linq
{
    class SolrQueryExpressionVisitor : ExpressionVisitor
    {
        private SolrQueryRequest _result;
        private readonly SolrLuceneExpressionVisitor _luceneExpressionVisitor;

        public SolrQueryExpressionVisitor(ISolrFieldResolver fieldResolver)
        {
            _luceneExpressionVisitor = new SolrLuceneExpressionVisitor(fieldResolver);
        }

        public SolrQueryRequest Translate(Expression expression)
        {
            _result = new SolrQueryRequest();
            Visit(expression);
            return _result;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;
            var solrLinqType = typeof (SolrQueryExtensions);
            if (method.DeclaringType == solrLinqType)
            {
                // first argument is the linq'ed expression, so it must be evaluated first
                Visit(node.Arguments[0]);
                switch (method.Name)
                {
                    case "For":
                        _result.Json.Query = _luceneExpressionVisitor.Translate(node.Arguments[1]);
                        _result.QueryType = node.Arguments[2].Invoke<string>();
                        break;
                    case "Filter":
                        _result.Json.Filters.Add(_luceneExpressionVisitor.Translate(node.Arguments[1]));
                        break;
                    default:
                        throw NotSupported();
                }
                return node;
            }
            if (method.DeclaringType == typeof (System.Linq.Queryable))
            {
                // first argument is the linq'ed expression, so it must be evaluated first
                Visit(node.Arguments[0]);
                switch (method.Name)
                {
                    case "Take":
                        _result.Json.Limit = node.Arguments[1].Invoke<int>();
                        break;
                    case "Skip":
                        _result.Json.Offset = node.Arguments[1].Invoke<int>();
                        break;
                    default:
                        throw NotSupported();
                }
                return node;
            }
            throw NotSupported();
        }

        private Exception NotSupported()
        {
            throw new NotSupportedException();
        }
    }
}
