using System;
using System.Linq;
using System.Linq.Expressions;

namespace AzureSearchToolkit.Request.Visitors
{
    /// <summary>
    /// Determines which part of the tree can be locally
    /// evaluated before execution and substitutes those parts
    /// with constant values obtained from local execution of that part.
    /// </summary>
    static class PartialEvaluator
    {
        static readonly Type[] doNotEvaluateMembersDeclaredOn = new Type[0];
        static readonly Type[] doNotEvaluateMethodsDeclaredOn = { typeof(Enumerable), typeof(Queryable), typeof(AzureSearchQueryExtensions), typeof(AzureSearchMethods) };

        public static Expression Evaluate(Expression e)
        {
            var chosenForEvaluation = BranchSelectExpressionVisitor.Select(e, ShouldEvaluate);

            return EvaluatingExpressionVisitor.Evaluate(e, chosenForEvaluation);
        }

        internal static bool ShouldEvaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Parameter || e.NodeType == ExpressionType.Lambda)
            {
                return false;
            }

            if (e is MemberExpression && doNotEvaluateMembersDeclaredOn.Contains(((MemberExpression)e).Member.DeclaringType) ||
               (e is MethodCallExpression && doNotEvaluateMethodsDeclaredOn.Contains(((MethodCallExpression)e).Method.DeclaringType)))
            {
                return false;
            }

            return true;
        }
    }
}
