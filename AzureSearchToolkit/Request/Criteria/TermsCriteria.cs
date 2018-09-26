using AzureSearchToolkit.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace AzureSearchToolkit.Request.Criteria
{
    /// <summary>
    /// Criteria that specifies one or more possible values that a
    /// field must match in order to select a document.
    /// </summary>
    public class TermsCriteria : SingleFieldCriteria, INegatableCriteria, ITermsCriteria
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TermsCriteria"/> class.
        /// </summary>
        /// <param name="executionMode">Type of execution mode this terms criteria will take.</param>
        /// <param name="field">Field to be checked for this term.</param>
        /// <param name="member">Property or field being checked for this term.</param>
        /// <param name="values">Constant values being searched for.</param>
        TermsCriteria(TermsOperator executionMode, string field, MemberInfo member, IEnumerable<object> values)
            : base(field)
        {
            Operator = executionMode;
            Member = member;
            Values = new ReadOnlyCollection<object>(values.ToArray());
        }

        /// <summary>
        /// Type of execution mode this terms criteria will take.
        /// </summary>
        public TermsOperator Operator { get; }

        bool ITermsCriteria.IsAnyCriteria => Operator == TermsOperator.Any;

        /// <summary>
        /// Property or field being checked for this term.
        /// </summary>
        public MemberInfo Member { get; }

        /// <inheritdoc/>
        public override string Name => "terms";

        /// <summary>
        /// Constant values being searched for.
        /// </summary>
        public ReadOnlyCollection<object> Values { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            var termOperator = "";
            var notOperator = false;

            if (Operator == TermsOperator.Any)
            {
                termOperator = "any";

                if (Values.Count > 1)
                {
                    return $"search.in({Field}, '{string.Join("|", Values)}, '|')";
                }
            }
            else if (Operator == TermsOperator.All || Operator == TermsOperator.NotAll)
            {
                termOperator = "all";

                if (Operator == TermsOperator.NotAll)
                {
                    notOperator = true;
                }
            }
            else
            {
                throw new KeyNotFoundException($"Term operator {Operator} not mapped!");
            }

            var filter = $"{Field}/{termOperator}(t: ";

            if (Values.Count == 1)
            {
                filter += $"t {(notOperator ? "ne " : "eq")} {ValueHelper.ConvertToSearchSafeValue(Values.First())}";
            }
            else
            {
                filter += $"{(notOperator ? "not " : "")}search.in(t, '{string.Join(",", Values)}')";
            }

            return filter + ")";
        }

        /// <summary>
        /// Builds a <see cref="TermsCriteria"/>
        /// </summary>
        /// <param name="field">The field that's being searched.</param>
        /// <param name="member">The member information for the field.</param>
        /// <param name="values">The values to be matched.</param>
        /// <returns>Either a <see cref="TermCriteria"/> object or a <see cref="TermsCriteria"/> object.</returns>
        internal static ICriteria Build(string field, MemberInfo member, params object[] values)
        {
            return Build(field, member, values.AsEnumerable());
        }

        /// <summary>
        /// Builds a <see cref="TermCriteria"/> or <see cref="TermsCriteria"/>, depending on how many values are
        /// present in the <paramref name="values"/> collection.
        /// </summary>
        /// <param name="field">The field that's being searched.</param>
        /// <param name="member">The member information for the field.</param>
        /// <param name="values">The values to be matched.</param>
        /// <returns>Returns a <see cref="TermsCriteria"/> object.</returns>
        internal static ICriteria Build(string field, MemberInfo member, IEnumerable<object> values)
        {
            return Build(TermsOperator.Any, field, member, values);
        }

        /// <summary>
        /// Builds a <see cref="TermsCriteria"/>
        /// </summary>
        /// <param name="termsOperator">The terms operator.</param>
        /// <param name="field">The field that's being searched.</param>
        /// <param name="member">The member information for the field.</param>
        /// <param name="values">The values to be matched.</param>
        /// <returns>Returns a <see cref="TermsCriteria"/> object.</returns>
        internal static ICriteria Build(TermsOperator termsOperator, string field, MemberInfo member, params object[] values)
        {
            return Build(termsOperator, field, member, values.AsEnumerable());
        }

        /// <summary>
        /// Builds a  <see cref="TermsCriteria"/>
        /// </summary>
        /// <param name="termsOperator">The terms execution mode (optional). Only used when a <see cref="TermsCriteria"/> is returned.</param>
        /// <param name="field">The field that's being searched.</param>
        /// <param name="member">The member information for the field.</param>
        /// <param name="values">The values to be matched.</param>
        /// <returns>Returns a <see cref="TermsCriteria"/> object.</returns>
        internal static ICriteria Build(TermsOperator termsOperator, string field, MemberInfo member, IEnumerable<object> values)
        {
            Argument.EnsureNotNull(nameof(values), values);

            var hashValues = new HashSet<object>(values);

            return new TermsCriteria(termsOperator, field, member, hashValues);
        }

        /// <inheritdoc/>
        public ICriteria Negate()
        {
            return new TermsCriteria(TermsOperator.NotAll, Field, Member, Values);
        }
    }
}
