using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Utilities
{
    static class EnumerableExtensions
    {
        public static ReadOnlyBatchedList<T> ToReadOnlyBatchedList<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyBatchedList<T>(enumerable);
        }
    }
}
