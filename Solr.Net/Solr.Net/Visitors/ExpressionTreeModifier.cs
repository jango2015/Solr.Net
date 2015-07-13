using System.Linq;
using System.Linq.Expressions;

namespace Solr.Net.Visitors
{
    internal class ExpressionTreeModifier<T> : ExpressionVisitor
    {
        private IQueryable _queryablePlaces;
        
        public void SetReplace(IQueryable<T> places)
        {
            _queryablePlaces = places;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            // Replace the constant queryable arg with the queryable Place collection.
            return c.Type == typeof(SolrQueryable<T>) ? Expression.Constant(_queryablePlaces) : c;
        }
    }

}
