using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AzureSearchToolkit.Utilities
{
    /// <summary>
    /// Various methods to make value formating for AzureSearch a little
    /// easier.
    /// </summary>
    static class ValueHelper
    {
        /// <summary>
        /// Get the converted safe value to be used in AzureSearch filter
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>Return converted safet value.</returns>
        public static string ConvertToSearchSafeValue(object value)
        {
            var valueText = string.Empty;

            if (value == null)
            {
                valueText = "null";
            }
            else
            {
                valueText = value.ToString();

                if (value is string || value is Guid)
                {
                    valueText = "'" + valueText + "'";
                }
                else if (value is double)
                {
                    valueText = ((double)value).ToString(CultureInfo.InvariantCulture);
                }
                else if (value is bool)
                {
                    valueText = value.ToString().ToLowerInvariant();
                }
                else if (value is DateTime)
                {
                    //valueText = ((DateTime)value).ToUniversalTime().ToString("o");
                    valueText = ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ssZ");
                    //valueText = ((DateTime)value).ToUniversalTime().ToString("O");
                }
                else if (value is DateTimeOffset)
                {
                    //valueText = ((DateTimeOffset)value).ToUniversalTime().ToString("o"); //.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    valueText = ((DateTimeOffset)value).ToString("yyyy-MM-ddTHH:mm:sszzz");
                    //valueText = ((DateTimeOffset)value).ToUniversalTime().ToString("O");
                }
            }

            return valueText;
        }
    }
}
