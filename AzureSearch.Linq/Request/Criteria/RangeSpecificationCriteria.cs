using AzureSearch.Linq.Utilities;
using System.Collections.Generic;
using System.Diagnostics;

namespace AzureSearch.Linq.Request.Criteria
{
    [DebuggerDisplay("{Name,nq} {Value}")]
    class RangeSpecificationCriteria : ICriteria
    {
        static readonly Dictionary<RangeComparison, string> rangeComparisonValues = new Dictionary<RangeComparison, string>
        {
            { RangeComparison.GreaterThan, "gt" },
            { RangeComparison.GreaterThanOrEqual, "gte" },
            { RangeComparison.LessThan, "lt" },
            { RangeComparison.LessThanOrEqual, "lte" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeSpecificationCriteria"/> class.
        /// </summary>
        /// <param name="comparison">Type of comparison for this range specification.</param>
        /// <param name="value">Constant value that this range specification tests against.</param>
        public RangeSpecificationCriteria(RangeComparison comparison, object value)
        {
            Argument.EnsureIsDefinedEnum(nameof(comparison), comparison);
            Argument.EnsureNotNull(nameof(value), value);

            Comparison = comparison;
            Value = value;
        }

        /// <summary>
        /// Type of comparison for this range specification.
        /// </summary>
        public RangeComparison Comparison { get; }

        /// <inheritdoc/>
        public string Name => rangeComparisonValues[Comparison];

        /// <summary>
        /// Constant value that this range specification tests against.
        /// </summary>
        public object Value { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Comparison} {Value}";
        }
    }
}
