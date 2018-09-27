using AzureSearchToolkit.Request;
using AzureSearchToolkit.Utilities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AzureSearchToolkit
{
    /// <summary>
    /// Extension methods that extend LINQ functionality for AzureSearch queries.
    /// </summary>
    /// <remarks>
    /// Using these methods against any provider except <see cref="AzureSearchQueryProvider"/> will fail.
    /// </remarks>
    public static class AzureSearchQueryExtensions
    {
        /// <summary>
        /// Creates an expression to call a generic version of the given method with the source and arguments as parameters..
        /// </summary>
        /// <typeparam name="TSource">Element type of the query derived from the IQueryable source.</typeparam>
        /// <param name="source">IQueryable source to use as the first parameter for the given method.</param>
        /// <param name="method">MethodInfo of the method to call.</param>
        /// <param name="arguments">Expressions that should be passed to the method as arguments.</param>
        /// <returns>IQueryable that contains the query with the method call inserted into the query chain.</returns>
        static IQueryable<TSource> CreateQueryMethodCall<TSource>(IQueryable<TSource> source, MethodInfo method, params Expression[] arguments)
        {
            Argument.EnsureNotNull(nameof(source), source);
            Argument.EnsureNotNull(nameof(method), source);

            var callExpression = Expression.Call(null, method.MakeGenericMethod(typeof(TSource)), new[] { source.Expression }.Concat(arguments));
            return source.Provider.CreateQuery<TSource>(callExpression);
        }

        static IQueryable<TSource> CreateQueryMethodCall<TSource, TKey>(IQueryable<TSource> source, MethodInfo method, params Expression[] arguments)
        {
            Argument.EnsureNotNull(nameof(source), source);
            Argument.EnsureNotNull(nameof(method), source);

            var callExpression = Expression.Call(null, method.MakeGenericMethod(typeof(TSource), typeof(TKey)), new[] { source.Expression }.Concat(arguments));
            return source.Provider.CreateQuery<TSource>(callExpression);
        }
    }
}
