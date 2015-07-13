using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Solr.Net.Helpers;
using Solr.Net.Visitors;

namespace Solr.Net
{
    internal class SolrQueryContext
    {
        internal static object Execute(Expression expression, bool isEnumerable)
        {
            // The expression must represent a query over the data source. 
            var elementType = TypeHelper.GetElementType(expression.Type);
            if (!IsQueryOverDataSource(expression))
            {
                throw new NotImplementedException("No query over the data source was specified.");
            }

            // Find the call to Where() and get the lambda expression predicate.
            var whereFinder = new InnermostWhereFinder();
            var whereExpression = whereFinder.GetInnermostWhere(expression);
            var lambdaExpression = (LambdaExpression)((UnaryExpression)(whereExpression.Arguments[1])).Operand;

            // Send the lambda expression through the partial evaluator.
            lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            // Get the search filters
            var binaryExpressionFinder = new BinaryExpressionFinder(lambdaExpression.Body);
            var filters = binaryExpressionFinder.Filters;

            // Call the Web service and get the results.
            var places = typeof(WebServiceHelper).GetMethod("GetResultsFromServerAsQueryable")
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] {filters});
            
            // Copy the expression tree that was passed in, changing only the first 
            // argument of the innermost MethodCallExpression.
            var treeCopierType = typeof (ExpressionTreeModifier<>).MakeGenericType(elementType);
            var treeCopier = Activator.CreateInstance(treeCopierType);
            treeCopierType.GetMethod("SetReplace").Invoke(treeCopier, new[] { places });
            var newExpressionTree =
                (Expression)treeCopierType.GetMethod("Visit", new[] { typeof(Expression) }).Invoke(treeCopier, new object[] { expression });
            // This step creates an IQueryable that executes by replacing Queryable methods with Enumerable methods. 
            var queryablePlaces = (IQueryable) places;
            return isEnumerable
                ? queryablePlaces.Provider.CreateQuery(newExpressionTree)
                : queryablePlaces.Provider.Execute(newExpressionTree);
        }
        private static bool IsQueryOverDataSource(Expression expression)
        {
            // If expression represents an unqueried IQueryable data source instance, 
            // expression is of type ConstantExpression, not MethodCallExpression. 
            return (expression is MethodCallExpression);
        }
    }
}