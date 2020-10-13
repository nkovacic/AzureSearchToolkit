using System.Linq;
using System.Linq.Expressions;

namespace AzureSearchToolkit.Request.Visitors
{
    class QuerySourceExpressionVisitor : ExpressionVisitor
    {
        IQueryable sourceQueryable;

        QuerySourceExpressionVisitor()
        {
        }

        public static IQueryable FindSource(Expression e)
        {
            var visitor = new QuerySourceExpressionVisitor();

            visitor.Visit(e);

            return visitor.sourceQueryable;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable)
            {
                sourceQueryable = ((IQueryable)node.Value);
            }

            return node;
        }
    }
}
