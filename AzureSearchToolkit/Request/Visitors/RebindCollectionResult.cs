using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace AzureSearchToolkit.Request.Visitors
{
    class RebindCollectionResult<T>
    {
        public RebindCollectionResult(Expression expression, IEnumerable<T> collected, ParameterExpression parameter)
        {
            Expression = expression;
            Collected = new ReadOnlyCollection<T>(collected.ToArray());
            Parameter = parameter;
        }

        public Expression Expression { get; }

        public ParameterExpression Parameter { get; }

        public ReadOnlyCollection<T> Collected { get; }
    }
}
