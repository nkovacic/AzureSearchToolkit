using Azure.Search.Documents.Models;
using AzureSearchToolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AzureSearchToolkit.Request.Visitors
{
    class MemberProjectionExpressionVisitor : AzureSearchFieldsExpressionVisitor
    {
        protected static readonly MethodInfo GetKeyedValueMethod = typeof(MemberProjectionExpressionVisitor).GetMethodInfo(m => m.Name == "GetKeyedValueOrDefault");

        readonly HashSet<string> fieldNames = new HashSet<string>();

        MemberProjectionExpressionVisitor(Type sourceType, ParameterExpression bindingParameter)
           : base(sourceType, bindingParameter)
        {
        }

        internal new static RebindCollectionResult<string> Rebind(Type sourceType, Expression selector)
        {
            var parameter = Expression.Parameter(typeof(SearchDocument), "h");
            var visitor = new MemberProjectionExpressionVisitor(sourceType, parameter);

            Argument.EnsureNotNull(nameof(selector), selector);

            var materializer = visitor.Visit(selector);

            return new RebindCollectionResult<string>(materializer, visitor.fieldNames, parameter);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
            {
                switch (node.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                    case ExpressionType.MemberAccess:
                        return VisitFieldSelection(node);
                }
            }

            return base.VisitMember(node);
        }

        Expression VisitFieldSelection(MemberExpression m)
        {
            var member = base.VisitAzureSearchField(m);

            fieldNames.Add(m.Member.Name.ToLowerInvariant());

            return member;
        }
    }
}
