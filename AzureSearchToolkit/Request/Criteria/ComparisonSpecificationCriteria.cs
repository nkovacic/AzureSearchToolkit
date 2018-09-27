using AzureSearchToolkit.Utilities;
using System.Collections.Generic;
using System.Diagnostics;

namespace AzureSearchToolkit.Request.Criteria
{
    [DebuggerDisplay("{Name,nq} {Value}")]
    class ComparisonSpecificationCriteria : INegatableCriteria, ICriteria
    {
        /// <summary>
        /// Type of comparison for this range specification.
        /// </summary>
        public Comparison Comparison { get; }

        /// <inheritdoc/>
        public string Name => ComparisonHelper.GetComparisonValue(Comparison);

        /// <summary>
        /// Constant value that this range specification tests against.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonSpecificationCriteria"/> class.
        /// </summary>
        /// <param name="comparison">Type of comparison for this range specification.</param>
        /// <param name="value">Constant value that this range specification tests against.</param>
        public ComparisonSpecificationCriteria(Comparison comparison, object value)
        {
            Argument.EnsureIsDefinedEnum(nameof(comparison), comparison);
            Argument.EnsureNotNull(nameof(value), value);

            Comparison = comparison;
            Value = value;
        }

        /// <inheritdoc/>
        public ICriteria Negate()
        {
            return new ComparisonSpecificationCriteria(ComparisonHelper.Invert(Comparison), Value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} {ValueHelper.ConvertToSearchSafeValue(Value)}";
        }
    }
}
