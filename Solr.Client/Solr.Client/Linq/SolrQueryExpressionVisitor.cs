using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Solr.Client.Serialization;
using Solr.Client.WebService;

namespace Solr.Client.Linq
{
    class SolrQueryExpressionVisitor : ExpressionVisitor
    {
        private SolrQueryRequest _result;
        protected readonly SolrLuceneExpressionVisitor LuceneExpressionVisitor;

        public SolrQueryExpressionVisitor(ISolrFieldResolver fieldResolver)
        {
            LuceneExpressionVisitor = new SolrLuceneExpressionVisitor(fieldResolver);
        }

        public SolrQueryExpressionVisitor(SolrLuceneExpressionVisitor luceneExpressionVisitor)
        {
            LuceneExpressionVisitor = luceneExpressionVisitor;
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
            var genericArguments = method.GetGenericArguments();
            if (method.DeclaringType == typeof (SolrQueryExtensions) || method.DeclaringType == typeof(Queryable))
            {
                // first argument is the linq'ed expression, so it must be evaluated first
                Visit(node.Arguments[0]);
                switch (method.Name)
                {
                    case "SearchFor":
                        _result.Json.Query = LuceneExpressionVisitor.Translate(node.Arguments[1]);
                        _result.QueryType = node.Arguments[2].Invoke<string>();
                        break;
                    case "QueryField":
                        _result.QueryFields.Add(LuceneExpressionVisitor.Translate(node.Arguments[1]));
                        break;
                    case "Filter":
                        _result.Json.Filters.Add(LuceneExpressionVisitor.Translate(node.Arguments[1]));
                        break;
                    case "TermsFacetFor":
                        var termsFacet = VisitTermsFacet(node);
                        _result.Json.Facets.Add(string.Format("T{0}", _result.Json.Facets.Count), termsFacet);
                        break;
                    case "RangeFacetFor":
                        var rangeFacet = VisitRangeFacet(node, genericArguments);
                        _result.Json.Facets.Add(string.Format("R{0}", _result.Json.Facets.Count), rangeFacet);
                        break;
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

        protected ISolrQueryFacet VisitRangeFacet(MethodCallExpression node, Type[] genericArguments)
        {
            var rangeFacetVisitor =
                Activator.CreateInstance(
                    typeof (SolrRangeFacetExpressionVisitor<>).MakeGenericType(genericArguments[1]),
                    new object[] {LuceneExpressionVisitor});
            var rangeFacet = (ISolrQueryFacet) rangeFacetVisitor.GetType()
                .GetMethod("TranslateFacet")
                .Invoke(rangeFacetVisitor, new object[] {node.Arguments[1], node.Arguments[2]});
            return rangeFacet;
        }

        protected ISolrQueryFacet VisitTermsFacet(MethodCallExpression node)
        {
            var termsFacetVisitor = new SolrTermsFacetExpressionVisitor(LuceneExpressionVisitor);
            var termsFacet = termsFacetVisitor.TranslateFacet(node.Arguments[1], node.Arguments[2]);
            return termsFacet;
        }

        protected static Exception NotSupported()
        {
            throw new NotSupportedException();
        }
    }

    internal class SolrRangeFacetExpressionVisitor<TRange> : SolrQueryExpressionVisitor
    {
        private SolrQueryRangeFacet<TRange> _result;

        public SolrRangeFacetExpressionVisitor(SolrLuceneExpressionVisitor luceneExpressionVisitor)
            : base(luceneExpressionVisitor)
        {
        }

        public SolrQueryRangeFacet<TRange> TranslateFacet(Expression fieldExpression,
            Expression optionsExpression)
        {
            _result = new SolrQueryRangeFacet<TRange> { Field = LuceneExpressionVisitor.Translate(fieldExpression) };
            Visit(optionsExpression);
            return _result;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;
            var genericArguments = method.GetGenericArguments();
            var solrLinqType = typeof(SolrQueryExtensions);
            if (method.DeclaringType == solrLinqType)
            {
                // first argument is the linq'ed expression, so it must be evaluated first
                Visit(node.Arguments[0]);
                switch (method.Name)
                {
                    case "Range":
                        _result.Start = node.Arguments[1].Invoke<TRange>();
                        _result.End = node.Arguments[2].Invoke<TRange>();
                        _result.Gap = node.Arguments[3].Invoke<string>();
                        break;
                    case "TermsFacetFor":
                        var termsFacet = VisitTermsFacet(node);
                        _result.Facets.Add(string.Format("T{0}", _result.Facets.Count), termsFacet);
                        break;
                    case "RangeFacetFor":
                        var rangeFacet = VisitRangeFacet(node, genericArguments);
                        _result.Facets.Add(string.Format("R{0}", _result.Facets.Count), rangeFacet);
                        break;
                    default:
                        throw NotSupported();
                }
                return node;
            }
            throw NotSupported();
        }
    }

    internal class SolrTermsFacetExpressionVisitor : SolrQueryExpressionVisitor
    {
        private SolrQueryTermsFacet _result;

        public SolrTermsFacetExpressionVisitor(SolrLuceneExpressionVisitor luceneExpressionVisitor)
            : base(luceneExpressionVisitor)
        {
        }

        public SolrQueryTermsFacet TranslateFacet(Expression fieldExpression,
            Expression optionsExpression)
        {
            _result = new SolrQueryTermsFacet { Field = LuceneExpressionVisitor.Translate(fieldExpression) };
            Visit(optionsExpression);
            return _result;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;
            var genericArguments = method.GetGenericArguments();
            var solrLinqType = typeof(SolrQueryExtensions);
            if (method.DeclaringType == solrLinqType)
            {
                // first argument is the linq'ed expression, so it must be evaluated first
                Visit(node.Arguments[0]);
                switch (method.Name)
                {
                    case "TermsFacetFor":
                        var termsFacet = VisitTermsFacet(node);
                        _result.Facets.Add(string.Format("T{0}", _result.Facets.Count), termsFacet);
                        break;
                    case "RangeFacetFor":
                        var rangeFacet = VisitRangeFacet(node, genericArguments);
                        _result.Facets.Add(string.Format("R{0}", _result.Facets.Count), rangeFacet);
                        break;
                    default:
                        throw NotSupported();
                }
                return node;
            }
            throw NotSupported();
        }
    }
}
