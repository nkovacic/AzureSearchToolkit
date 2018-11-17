using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest
{
    static class DataAssert
    {
        public static readonly Data Data = new Data();

        static DataAssert()
        {
            Data.LoadFromJsonToAzureSearch();
            Data.LoadToMemoryFromAzureSearch();
        }

        public static void Same<TSource>(Func<IQueryable<TSource>, IQueryable<TSource>> query, bool ignoreOrder = false) where TSource : class
        {
            Same<TSource, TSource>(query, ignoreOrder);
        }

        public static void Same<TSource, TTarget>(Func<IQueryable<TSource>, IQueryable<TTarget>> query, bool ignoreOrder = false) where TSource : class
        {
            var expect = query(Data.Memory<TSource>()).ToList();
            var actual = query(Data.SearchQuery<TSource>()).ToList();

            Same(expect, actual, ignoreOrder);
        }

        public static void Same<TTarget>(IEnumerable<TTarget> expect, IEnumerable<TTarget> actual, bool ignoreOrder = false)
        {
            if (ignoreOrder)
            {
                var difference = Difference(expect, actual);

                Assert.Empty(difference);
            }
            else
            {
                SameSequence(expect.ToList(), actual.ToList());
            }
        }

        public static void SameSequence<TTarget>(List<TTarget> expect, List<TTarget> actual)
        {
            Assert.Equal(expect.Count, actual.Count);

            var upperBound = Math.Min(expect.Count, actual.Count);

            for (var i = 0; i < upperBound; i++)
            {
                Assert.Equal(expect[i], actual[i]);
            }
        }

        static IEnumerable<T> Difference<T>(IEnumerable<T> left, IEnumerable<T> right)
        {
            var rightCache = new HashSet<T>(right);

            rightCache.SymmetricExceptWith(left);

            return rightCache;
        }
    }
}
