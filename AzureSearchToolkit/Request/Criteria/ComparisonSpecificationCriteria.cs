using AzureSearchToolkit.Utilities;
using System.Collections.Generic;
using System.Diagnostics;

namespace AzureSearchToolkit.Request.Criteria
{
    [DebuggerDisplay("{Name,nq} {Value}")]
    class ComparisonSpecificationCriteria : INegatableCriteria, ICriteria
    {
        static readonly Dictionary<Comparison, string> rangeComparisonValues = new Dictionary<Comparison, string>
        {
            { Comparison.Equal, "eq" },
            { Comparison.GreaterThan, "gt" },
            { Comparison.GreaterThanOrEqual, "ge" },
            { Comparison.NotEqual, "ne" },
            { Comparison.LessThan, "lt" },
            { Comparison.LessThanOrEqual, "le" },
        };

        static readonly Comparison[] invertedComparison =
        {
            Comparison.NotEqual,
            Comparison.LessThan,
            Comparison.LessThanOrEqual,
            Comparison.NotEqual,
            Comparison.GreaterThan,
            Comparison.GreaterThanOrEqual
        };

        /// <summary>
        /// Type of comparison for this range specification.
        /// </summary>
        public Comparison Comparison { get; }

        /// <inheritdoc/>
        public string Name => rangeComparisonValues[Comparison];

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
            return new ComparisonSpecificationCriteria(invertedComparison[(int)Comparison], Value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} {ValueHelper.ConvertToSearchSafeValue(Value)}";
        }
    }
}
