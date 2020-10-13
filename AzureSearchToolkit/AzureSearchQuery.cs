using Azure.Search.Documents;
using AzureSearchToolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AzureSearchToolkit
{
    public class AzureSearchQuery<TSource, TTarget> : IAzureSearchQuery<TSource, TTarget>
    {
        readonly AzureSearchQueryProvider<TSource> provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSearchQuery{T}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="AzureSearchQueryProvider"/> used to execute the queries.</param>
        public AzureSearchQuery(AzureSearchQueryProvider<TSource> provider)
        {
            Argument.EnsureNotNull(nameof(provider), provider);

            this.provider = provider;
            Expression = Expression<TTarget>.Constant(this) as Expression<TTarget>;
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="provider">The <see cref="AzureSearchQueryProvider"/> used to execute the queries.</param>
        /// <param name="expression">The <see cref="Expression"/> that represents the LINQ query so far.</param>
        public AzureSearchQuery(AzureSearchQueryProvider<TSource> provider, Expression expression)
        {
            Argument.EnsureNotNull(nameof(provider), provider);
            Argument.EnsureNotNull(nameof(expression), expression);

            if (!typeof(IQueryable<TTarget>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }

            this.provider = provider;
            Expression = expression;
        }

        /// <inheritdoc/>
        public IEnumerator<TTarget> GetEnumerator()
        {
            return ((IEnumerable<TTarget>)provider.Execute(Expression)).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)provider.Execute(Expression)).GetEnumerator();
        }

        /// <inheritdoc/>
        public Type ElementType => typeof(TTarget);

        /// <inheritdoc/>
        public Expression Expression { get; }

        /// <inheritdoc/>
        public IQueryProvider Provider => provider;
    }
}
