using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit.Utilities
{
    static class EnumerableExtensions
    {
        public static SortedDictionary<T1, T2> ToSortedDictionary<T1, T2>(this IEnumerable<T2> source, Func<T2, T1> keySelector)
        {
            return new SortedDictionary<T1, T2>(source.ToDictionary(keySelector));
        }

        public static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return new SortedDictionary<TKey, TElement>(source.ToDictionary(keySelector, elementSelector));
        }

        public static ReadOnlyBatchedList<T> ToReadOnlyBatchedList<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyBatchedList<T>(enumerable);
        }
    }
}
