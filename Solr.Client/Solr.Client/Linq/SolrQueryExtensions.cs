using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Solr.Client.Linq
{
    public static class SolrQueryExtensions
    {
        public static IQueryable<TDocument> Filter<TDocument>(this IQueryable<TDocument> @this, string query)
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
        public static IQueryable<TDocument> Filter<TDocument>(this IQueryable<TDocument> @this, Expression<Func<TDocument, object>> expression)
        {
            return @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[] { @this.Expression, expression }));
        }

        public static IQueryable<TDocument> QueryField<TDocument>(this IQueryable<TDocument> @this, string field)
        {
            return @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TDocument)),
                    new[]
                    {
                        @this.Expression,
                        Expression.Call(typeof (SolrLiteral).GetMethod("String"), Expression.Constant(field))
                    }));
        }

        public static IQueryable<TDocument> QueryField<TDocument>(this IQueryable<TDocument> @this, Expression<Func<TDocument, object>> expression)
        {
            return @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[] { @this.Expression, expression }));
        }

        public static IQueryable<TDocument> For<TDocument>(this IQueryable<TDocument> @this,
            Expression<Func<TDocument, object>> expression, string queryType = "lucene")
        {
            return @this.Provider.CreateQuery<TDocument>(
                Expression.Call(
                    null,
                    ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDocument)),
                    new[] { @this.Expression, expression, Expression.Constant(queryType) }));
        }

        public static IQueryable<TDocument> For<TDocument>(this IQueryable<TDocument> @this,
            string query, string queryType = "dismax")
        {
            return @this.Provider.CreateQuery<TDocument>(
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

        public static T Compile<T>(this Expression @this)
        {
            return Expression.Lambda<T>(@this).Compile();
        }

        public static T Invoke<T>(this Expression @this)
        {
            return Expression.Lambda<Func<T>>(@this).Compile()();
        }
    }
}
