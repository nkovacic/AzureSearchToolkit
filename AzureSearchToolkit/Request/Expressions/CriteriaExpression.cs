using AzureSearchToolkit.Request.Criteria;
using System;
using System.Linq.Expressions;

namespace AzureSearchToolkit.Request.Expressions
{
    class CriteriaExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CriteriaExpression"/> class.
        /// </summary>
        /// <param name="criteria"><see cref="ICriteria" /> to represent with this expression.</param>
        public CriteriaExpression(ICriteria criteria)
        {
            Criteria = criteria;
        }

        /// <summary>
        /// <see cref="ICriteria" /> that is represented by this expression.
        /// </summary>
        public ICriteria Criteria { get; }

        /// <inheritdoc/>
        public override ExpressionType NodeType => AzureSearchExpressionType.Criteria;

        /// <inheritdoc/>
        public override Type Type => typeof(bool);

        /// <inheritdoc/>
        public override bool CanReduce => false;

        /// <inheritdoc/>
        public override string ToString()
        {
            return Criteria.ToString();
        }
    }
}
