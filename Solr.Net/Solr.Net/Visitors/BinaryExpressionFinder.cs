using System.Collections.Generic;
using System.Linq.Expressions;

namespace Solr.Net.Visitors
{
    class BinaryExpressionFinder : ExpressionVisitor
    {
        private readonly Expression _expression;
        private Dictionary<string, object> _filters;

        public BinaryExpressionFinder(Expression exp)
        {
            _expression = exp;
        }

        public Dictionary<string, object> Filters
        {
            get
            {
                if (_filters != null) return _filters;
                _filters = new Dictionary<string, object>();
                Visit(_expression);
                return _filters;
            }
        }

        protected override Expression VisitBinary(BinaryExpression be)
        {
            // find membername
            string memberName = null;
            if (be.Left.NodeType == ExpressionType.MemberAccess)
            {
                memberName = ((MemberExpression) be.Left).Member.Name;
            }
            else if (be.Right.NodeType == ExpressionType.MemberAccess)
            {
                memberName = ((MemberExpression)be.Right).Member.Name;
            }
            // find value
            object value = null;
            if (be.Right.NodeType == ExpressionType.Constant)
            {
                value = ((ConstantExpression)be.Right).Value;
            }
            if (be.Left.NodeType == ExpressionType.Constant)
            {
                value = ((ConstantExpression)be.Left).Value;
            }
            // verify member expression was found
            if (memberName == null || value == null)
            {
                return base.VisitBinary(be);
            }
            // add filter
            // TODO: Handle expression types, e.g. be.NodeType == ExpressionType.Equal
            if (_filters.ContainsKey(memberName))
            {
                _filters.Add(memberName, value);
            }
            else
            {
                _filters[memberName] = value;
            }
            return be;
        }
    }
}
