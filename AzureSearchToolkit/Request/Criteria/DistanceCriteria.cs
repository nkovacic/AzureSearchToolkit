using AzureSearchToolkit.Utilities;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AzureSearchToolkit.Request.Criteria
{
    class DistanceCriteria : INegatableCriteria, ICriteria
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistanceCriteria"/> class.
        /// </summary>
        /// <param name="field">Field that must be within the specified ranges.</param>
        /// <param name="member">Property or field that this range criteria applies to.</param>
        /// <param name="value">GeographyPoint for distance calculation</param>
        /// <param name="comparisonValue">ComparisonSpecificationCriteria that this criteria tests against.</param>
        public DistanceCriteria(string field, MemberInfo member, GeographyPoint value, ComparisonSpecificationCriteria comparisonValue)
        {
            Argument.EnsureNotBlank(nameof(field), field);
            Argument.EnsureNotNull(nameof(member), member);
            //Argument.EnsureNotNull(nameof(comparisonValue), comparisonValue);

            Field = field;
            Member = member;
            CriteriaForComparison = comparisonValue;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistanceCriteria"/> class.
        /// </summary>
        /// <param name="field">Field that must be within the specified ranges.</param>
        /// <param name="member">Property or field that this range criteria applies to.</param>
        /// <param name="value">GeographyPoint for distance calculation</param>
        /// <param name="comparison">Type of comparison</param>
        /// <param name="comparisonValue">Value to compare</param>
        public DistanceCriteria(string field, MemberInfo member, GeographyPoint value, Comparison comparison, object comparisonValue)
            : this(field, member, value, new ComparisonSpecificationCriteria(comparison, comparisonValue))
        {
            
        }

        /// <summary>
        /// Property or field that this range criteria applies to.
        /// </summary>
        public MemberInfo Member { get; }

        /// <inheritdoc/>
        public string Name => "distance";

        /// <summary>
        /// Field that must be within the specified ranges.
        /// </summary>
        public string Field { get; }

        public GeographyPoint Value { get; set; }

        /// <summary>
        /// Comparison for DistanceCriteria.
        /// </summary>
        public ComparisonSpecificationCriteria CriteriaForComparison { get; private set; }

        public ICriteria Negate()
        {
            return new DistanceCriteria(Field, Member, Value, CriteriaForComparison);
        }

        public void ReplaceComparison(Comparison comparison, object value)
        {
            CriteriaForComparison = new ComparisonSpecificationCriteria(comparison, value);
        }

        public override string ToString()
        {
            return $"geo.distance({Field}, geography'POINT({ValueHelper.ConvertToSearchSafeValue(Value.Longitude)} "
                + $"{ValueHelper.ConvertToSearchSafeValue(Value.Latitude)})') {CriteriaForComparison.ToString()}";
        }
    }
}
