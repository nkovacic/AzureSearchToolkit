using AzureSearch.Linq.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearch.Linq.Mapping
{
    /// <summary>
    /// Common techniques for re-mapping names used between the various mappings.
    /// </summary>
    public static class MappingHelper
    {
        private static readonly char[] Delimeters = { ' ', '-', '_' };

        public static string ToCamelCase(string source)
        {
            Argument.EnsureNotNull(nameof(source), source);

            return SymbolsPipe(
                source,
                '\0',
                (s, disableFrontDelimeter) =>
                {
                    if (disableFrontDelimeter)
                    {
                        return new char[] { char.ToLowerInvariant(s) };
                    }

                    return new char[] { char.ToUpperInvariant(s) };
                });
        }

        private static string SymbolsPipe(string source, char mainDelimeter, Func<char, bool, char[]> newWordSymbolHandler)
        {
            var builder = new StringBuilder();

            var nextSymbolStartsNewWord = true;
            var disableFrontDelimeter = true;

            for (var i = 0; i < source.Length; i++)
            {
                var symbol = source[i];
                if (Delimeters.Contains(symbol))
                {
                    if (symbol == mainDelimeter)
                    {
                        builder.Append(symbol);
                        disableFrontDelimeter = true;
                    }

                    nextSymbolStartsNewWord = true;
                }
                else if (!char.IsLetterOrDigit(symbol))
                {
                    builder.Append(symbol);
                    disableFrontDelimeter = true;
                    nextSymbolStartsNewWord = true;
                }
                else
                {
                    if (nextSymbolStartsNewWord || char.IsUpper(symbol))
                    {
                        builder.Append(newWordSymbolHandler(symbol, disableFrontDelimeter));
                        disableFrontDelimeter = false;
                        nextSymbolStartsNewWord = false;
                    }
                    else
                    {
                        builder.Append(symbol);
                    }
                }
            }

            return builder.ToString();
        }
    }
}
