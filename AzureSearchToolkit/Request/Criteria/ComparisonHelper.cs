using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Request.Criteria
{
    static class ComparisonHelper
    {
        static readonly Dictionary<Comparison, string> comparisonValues = new Dictionary<Comparison, string>
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

        public static string GetComparisonValue(Comparison comparison)
        {
            return comparisonValues[comparison];
        }

        public static Comparison Invert(Comparison comparison)
        {
            return invertedComparison[(int)comparison];
        }
    }
}
