using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AzureSearchToolkit.Request.Visitors
{
    /// <summary>
    /// Expression visitor that substitutes references to <see cref="Document"/>
    /// with desired type.
    /// </summary>
    class AzureSearchFieldsExpressionVisitor : ExpressionVisitor
    {
        protected readonly ParameterExpression BindingParameter;
        protected readonly IAzureSearchMapping Mapping;
        protected readonly Type SourceType;

        public AzureSearchFieldsExpressionVisitor(Type sourcetype, ParameterExpression bindingParameter, IAzureSearchMapping mapping)
        {
            Argument.EnsureNotNull(nameof(bindingParameter), bindingParameter);
            Argument.EnsureNotNull(nameof(mapping), mapping);

            SourceType = sourcetype;
            BindingParameter = bindingParameter;
            Mapping = mapping;
        }

        internal static Tuple<Expression, ParameterExpression> Rebind(Type sourceType, IAzureSearchMapping mapping, Expression selector)
        {
            var parameter = Expression.Parameter(typeof(Document), "h");
            var visitor = new AzureSearchFieldsExpressionVisitor(sourceType, parameter, mapping);

            Argument.EnsureNotNull(nameof(selector), selector);

            return Tuple.Create(visitor.Visit(selector), parameter);
        }

        protected virtual Expression VisitAzureSearchField(MemberExpression m)
        {
            return Expression.Convert(Expression.Property(BindingParameter, "Item", Expression.Constant(NameHelper.GetMemberName(m.Member))), m.Type);
        }
    }
}
