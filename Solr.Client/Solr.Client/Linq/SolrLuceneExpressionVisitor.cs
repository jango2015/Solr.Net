using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Solr.Client.Serialization;

namespace Solr.Client.Linq
{
    public class SolrLuceneExpressionVisitor : ExpressionVisitor
    {
        public readonly ISolrFieldResolver FieldResolver;
        private StringBuilder _query;

        public SolrLuceneExpressionVisitor(ISolrFieldResolver fieldResolver)
        {
            FieldResolver = fieldResolver;
        }

        public string Translate(Expression expression)
        {
            _query = new StringBuilder();
            Visit(expression);
            return _query.ToString();
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _query.Append("-");
                    Visit(node.Operand);
                    break;
                default:
                    //throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", node.NodeType));
                    return base.VisitUnary(node);
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // rewrite not equal
            if (node.NodeType == ExpressionType.NotEqual)
            {
                return
                    Visit(Expression.MakeUnary(ExpressionType.Not,
                        Expression.MakeBinary(ExpressionType.Equal, node.Left, node.Right), node.Type));
            }
            // handle
            _query.Append("(");
            Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    _query.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    _query.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    _query.Append(":");
                    break;
            }
            Visit(node.Right);
            _query.Append(")");
            // done
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
            {
                _query.Append("\"\"");
                return node;
            }
            var valueType = node.Value.GetType();
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    _query.Append(((bool)node.Value) ? 1 : 0);
                    break;
                case TypeCode.String:
                    _query.Append("\"");
                    _query.Append(((string)node.Value).Replace("\"", "\\\""));
                    _query.Append("\"");
                    break;
                default:
                    _query.Append(node.Value);
                    break;
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof (SolrLiteral) && node.Arguments.Count > 0)
            {
                // the argument is a literal string; compute it and print it
                var argument = Expression.Lambda(node.Arguments[0]).Compile().DynamicInvoke();
                _query.Append(argument);
                return node;
            }
            if (node.Method.Name == "Equals" || node.Method.Name == "Contains")
            {
                if (node.Arguments.Count >= 1)
                {
                    if (node.Object == null)
                    {
                        if (node.Arguments.Count >= 2)
                        {
                            Visit(Expression.MakeBinary(ExpressionType.Equal, node.Arguments[0], node.Arguments[1]));
                        }
                    }
                    else
                    {
                        Visit(Expression.MakeBinary(ExpressionType.Equal, node.Object, node.Arguments[0]));
                    }
                }
                return node;
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == null)
            {
                return base.VisitMember(node);
            }
            if (node.Expression.NodeType == ExpressionType.Parameter)
            {
                _query.Append(FieldResolver.GetFieldName(node.Member));
                return node;
            }
            if (node.Expression.NodeType == ExpressionType.Constant)
            {
                var container = ((ConstantExpression) node.Expression).Value;
                var fieldInfo = node.Member as FieldInfo;
                if (fieldInfo != null)
                {
                    VisitConstant(Expression.Constant(fieldInfo.GetValue(container)));
                    return node;
                }
                var propertyInfo = node.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    VisitConstant(Expression.Constant(propertyInfo.GetValue(container)));
                    return node;
                }
            }
            //throw new NotSupportedException(string.Format("The member '{0}' is not supported", node.Member.Name));
            return base.VisitMember(node);
        }
    }
}
