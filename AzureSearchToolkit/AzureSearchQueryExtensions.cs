using AzureSearchToolkit.Request;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
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
        /// Queries Azure Search with Simple Query Parser
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable`1"/> that contains elements from the input sequence that satisfy the condition specified by <paramref name="searchText"/>.
        /// </returns>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"/> to query.</param>
        /// <param name="searchText">A query text to test each element for.</param>
        /// <param name="searchMode">. The <see cref="SearchMode"/> parameter is used to match on any term (default) or all of them, for cases where a term is not explicitly required (+).</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="searchText"/> is null.</exception>
        public static IQueryable<TSource> SimpleQuery<TSource>(this IQueryable<TSource> source, string searchText, SearchMode searchMode = SearchMode.Any,
            params Expression<Func<TSource, object>>[] searchFields)
        {
            Argument.EnsureNotNull(nameof(searchText), searchText);

            return CreateQueryMethodCall(source, simpleQueryMethodInfo, Expression.Constant(searchText), Expression.Constant(searchMode), Expression.NewArrayInit(typeof(Expression<Func<TSource, object>>), searchFields));
        }

        /// <summary>
        /// Queries Azure Search with Lucene Query Parser
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable`1"/> that contains elements from the input sequence that satisfy the condition specified by <paramref name="searchText"/>.
        /// </returns>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"/> to query.</param>
        /// <param name="searchText">A query text to test each element for.</param>
        /// <param name="searchMode">. The <see cref="SearchMode"/> parameter is used to match on any term (default) or all of them, for cases where a term is not explicitly required (+).</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="searchText"/> is null.</exception>
        public static IQueryable<TSource> LuceneQuery<TSource>(this IQueryable<TSource> source, string searchText, SearchMode searchMode = SearchMode.Any,
            params Expression<Func<TSource, object>>[] searchFields)
        {
            Argument.EnsureNotNull(nameof(searchText), searchText);

            return CreateQueryMethodCall(source, luceneQueryMethodInfo, Expression.Constant(searchText), Expression.Constant(searchMode), Expression.NewArrayInit(typeof(Expression<Func<TSource, object>>), searchFields));
        }

        static readonly MethodInfo luceneQueryMethodInfo = typeof(AzureSearchQueryExtensions).GetMethodInfo(m => m.Name == "LuceneQuery" && m.GetParameters().Length >= 1);
        static readonly MethodInfo simpleQueryMethodInfo = typeof(AzureSearchQueryExtensions).GetMethodInfo(m => m.Name == "SimpleQuery" && m.GetParameters().Length >= 1);

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
