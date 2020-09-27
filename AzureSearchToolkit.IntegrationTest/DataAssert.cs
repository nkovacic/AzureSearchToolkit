using AzureSearchToolkit.IntegrationTest.Models;
using AzureSearchToolkit.IntegrationTest.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest
{
    static class DataAssert
    {
        private static readonly Dictionary<Type, string> DefaultOrders = new Dictionary<Type, string>();

        public static readonly Data Data = new Data();

        static DataAssert()
        {
            Data.LoadFromJsonToAzureSearch();
            Data.LoadToMemoryFromAzureSearch();

            SetDefaultOrderForType<Listing, DateTime?>(q => q.CreatedAt);
        }

        public static void SetDefaultOrderForType<TSource, TKey>(Expression<Func<TSource, TKey>> defaultOrderForType)
        {
            var propertyName = PropertyHelper.GetPropertyName(defaultOrderForType);

            if (!string.IsNullOrWhiteSpace(propertyName) && !DefaultOrders.ContainsKey(typeof(TSource)))
            {
                DefaultOrders.Add(typeof(TSource), propertyName);
            }
        }

        public static void Same<TSource>(Func<IQueryable<TSource>, IQueryable<TSource>> query,
            bool useDefaultOrder = true, bool ignoreOrder = false) where TSource : class
        {
            Same<TSource, TSource>(query, useDefaultOrder, ignoreOrder);
        }

        public static void Same<TSource, TTarget>(Func<IQueryable<TSource>, IQueryable<TTarget>> query,
            bool useDefaultOrder = true, bool ignoreOrder = false) where TSource : class
        {
            var expectQuery = query(Data.Memory<TSource>());
            var actualQuery = query(Data.Memory<TSource>());
            var type = typeof(TSource);

            List<TTarget> expect = null;
            List<TTarget> actual = null;

            if (useDefaultOrder && DefaultOrders.ContainsKey(type))
            {
                expect = ExpressionsHelper.OrderingHelper(query(Data.Memory<TSource>()), DefaultOrders[type]).ToList();
                actual = ExpressionsHelper.OrderingHelper(query(Data.SearchQuery<TSource>()), DefaultOrders[type]).ToList();
            }
            else
            {
                expect = query(Data.Memory<TSource>()).ToList();
                actual = query(Data.SearchQuery<TSource>()).ToList();
            }

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
                if (!expect[i].Equals(actual[i]))
                {
                    Assert.Equal(expect[i], actual[i]);
                }
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
