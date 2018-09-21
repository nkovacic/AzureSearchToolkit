using AzureSearchToolkit.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AzureSearchToolkit.Request.Criteria
{
    /// <summary>
    /// Criteria that specifies a value or range of desired values for a given
    /// field that need to be satisfied to select a document.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    class ComparisonCriteria : INegatableCriteria, ICriteria
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonCriteria"/> class.
        /// </summary>
        /// <param name="field">Field that must be within the specified ranges.</param>
        /// <param name="member">Property or field that this range criteria applies to.</param>
        /// <param name="specifications">Specifications (upper and lower bounds) that must be met.</param>
        public ComparisonCriteria(string field, MemberInfo member, IEnumerable<ComparisonSpecificationCriteria> specifications)
        {
            Argument.EnsureNotBlank(nameof(field), field);
            Argument.EnsureNotNull(nameof(member), member);
            Argument.EnsureNotNull(nameof(specifications), specifications);

            Field = field;
            Member = member;
            Specifications = new ReadOnlyCollection<ComparisonSpecificationCriteria>(specifications.ToArray());
        }

        public ComparisonCriteria(string field, MemberInfo member, Comparison comparison, object value)
            : this(field, member, new[] { new ComparisonSpecificationCriteria(comparison, value) }) { }

        /// <summary>
        /// Property or field that this range criteria applies to.
        /// </summary>
        public MemberInfo Member { get; }

        /// <inheritdoc/>
        public string Name => "comparison";

        /// <summary>
        /// Field that must be within the specified ranges.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// Specifications (upper and lower bounds) that must be met.
        /// </summary>
        public ReadOnlyCollection<ComparisonSpecificationCriteria> Specifications { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Join(" and ", Specifications.Select(s => $"{Field} {s.ToString()}"));
        }

        /// <summary>
        /// Determine whether a list of <see cref="ComparisonSpecificationCriteria" /> can be combined or not.
        /// </summary>
        /// <param name="specifications">List of <see cref="ComparisonSpecificationCriteria" />to be considered.</param>
        /// <returns><c>true</c> if they can be combined; otherwise <c>false</c>.</returns>
        internal static bool SpecificationsCanBeCombined(List<ComparisonSpecificationCriteria> specifications)
        {
            return specifications.Count(r => r.Comparison == Comparison.GreaterThan || r.Comparison == Comparison.GreaterThanOrEqual) < 2
                 && specifications.Count(r => r.Comparison == Comparison.LessThan || r.Comparison == Comparison.LessThanOrEqual) < 2;
        }

        internal static bool SpecificationsCanBeReduced(List<ComparisonSpecificationCriteria> specifications)
        {
            return specifications.GroupBy(q => q.Comparison).Any(q => q.Count() > 1);
        }

        /// <inheritdoc/>
        public ICriteria Negate()
        {
            return new ComparisonCriteria(Field, Member, Specifications.Select(q => (ComparisonSpecificationCriteria)q.Negate()));
        }
    }
}
