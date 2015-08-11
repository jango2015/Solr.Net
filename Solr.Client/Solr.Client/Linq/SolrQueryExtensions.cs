using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Solr.Client.Linq
{
    public static class SolrQueryExtensions
    {
        public static IQueryable<TDocument> Filter<TDocument>
            (this IQueryable<TDocument> @this,
            string query)
        {
            return @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TDocument)),
                    new[]
                    {
                        @this.Expression,
                        Expression.Call(typeof (SolrLiteral).GetMethod("String"), Expression.Constant(query)),
                    }));
        }
        public static IQueryable<TDocument> Filter<TDocument>(
            this IQueryable<TDocument> @this,
            Expression<Func<TDocument, object>> expression)
        {
            return @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[] { @this.Expression, expression }));
        }

        public static IQueryable<TDocument> QueryField<TDocument>(
            this IQueryable<TDocument> @this,
            string field)
        {
            return (IQueryable<TDocument>) @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TDocument)),
                    new[]
                    {
                        @this.Expression,
                        Expression.Call(typeof (SolrLiteral).GetMethod("String"), Expression.Constant(field))
                    }));
        }

        public static IQueryable<TDocument> QueryField<TDocument>(
            this IQueryable<TDocument> @this,
            Expression<Func<TDocument, object>> expression)
        {
            return (IQueryable<TDocument>)@this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[] { @this.Expression, expression }));
        }

        public static IQueryable<TDocument> SearchFor<TDocument>(
            this IQueryable<TDocument> @this,
            Expression<Func<TDocument, object>> expression,
            string queryType = "lucene")
        {
            return (IQueryable<TDocument>) @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[] { @this.Expression, expression, Expression.Constant(queryType) }));
        }

        public static IQueryable<TDocument> SearchFor<TDocument>(
            this IQueryable<TDocument> @this,
            string query,
            string queryType = "dismax")
        {
            return (IQueryable<TDocument>) @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TDocument)),
                    new[]
                    {
                        @this.Expression,
                        Expression.Call(typeof (SolrLiteral).GetMethod("String"), Expression.Constant(query)),
                        Expression.Constant(queryType)
                    }));
        }

        public static IQueryable<TDocument> TermsFacetFor<TDocument>(
            this IQueryable<TDocument> @this,
            Expression<Func<TDocument, object>> field)
        {
            return @this.TermsFacetFor(field, query => null);
        }

        public static IQueryable<TDocument> TermsFacetFor<TDocument>(
            this IQueryable<TDocument> @this,
            Expression<Func<TDocument, object>> field,
            Expression<Func<SolrTermsFacetQuery<TDocument>, object>> options)
        {
            return
                @this.Provider.CreateQuery<TDocument>(Expression.Call(null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[]
                    {
                        @this.Expression,
                        field,
                        options
                    }));
        }

        public static IQueryable<TDocument> RangeFacetFor<TDocument, TRange>(
            this IQueryable<TDocument> @this,
            Expression<Func<TDocument, TRange>> field,
            Expression<Func<SolrRangeFacetQuery<TDocument, TRange>, object>> options)
        {
            return
                @this.Provider.CreateQuery<TDocument>(Expression.Call(null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TDocument), typeof (TRange)),
                    new[]
                    {
                        @this.Expression,
                        field,
                        (Expression) options ??
                        Expression.Constant(null,
                            typeof (Expression<Func<SolrRangeFacetQuery<TDocument, TRange>, object>>))
                    }));
        }

        public static SolrRangeFacetQuery<TDocument, TRange> Range<TDocument, TRange>(
            this SolrRangeFacetQuery<TDocument, TRange> @this,
            TRange from,
            TRange to,
            string gap)
        {
            return (SolrRangeFacetQuery<TDocument, TRange>)@this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[]
                    {
                        @this.Expression,
                        Expression.Convert(Expression.Constant(from), typeof (TRange)),
                        Expression.Convert(Expression.Constant(to), typeof (TRange)),
                        Expression.Convert(Expression.Constant(gap), typeof (TRange))
                    }));
        }

        public static T Compile<T>(this Expression @this)
        {
            return Expression.Lambda<T>(@this).Compile();
        }

        public static T Invoke<T>(this Expression @this)
        {
            return Expression.Lambda<Func<T>>(@this).Compile()();
        }

        public static object Invoke(this Expression @this)
        {
            return Expression.Lambda(@this).Compile().DynamicInvoke();
        }
    }
}
