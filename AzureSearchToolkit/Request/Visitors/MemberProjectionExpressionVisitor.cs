using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace AzureSearchToolkit.Request.Visitors
{
    class MemberProjectionExpressionVisitor : AzureSearchFieldsExpressionVisitor
    {
        protected static readonly MethodInfo GetKeyedValueMethod = typeof(MemberProjectionExpressionVisitor).GetMethodInfo(m => m.Name == "GetKeyedValueOrDefault");

        readonly HashSet<string> fieldNames = new HashSet<string>();

        MemberProjectionExpressionVisitor(Type sourceType, ParameterExpression bindingParameter, IAzureSearchMapping mapping)
           : base(sourceType, bindingParameter, mapping)
        {
        }

        internal new static RebindCollectionResult<string> Rebind(Type sourceType, IAzureSearchMapping mapping, Expression selector)
        {
            var parameter = Expression.Parameter(typeof(Document), "h");
            var visitor = new MemberProjectionExpressionVisitor(sourceType, parameter, mapping);

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

            fieldNames.Add(NameHelper.GetMemberName(m.Member));

            return member;
        }
    }
}
