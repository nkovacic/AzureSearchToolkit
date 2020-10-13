using Azure;
using Azure.Search.Documents.Models;
using AzureSearchToolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchToolkit.Response.Materializers
{
    /// <summary>
    /// Materializes multiple hits into a list of CLR objects.
    /// </summary>
    class ListAzureSearchMaterializer<T> : IAzureSearchMaterializer<T>
    {
        static readonly MethodInfo manyMethodInfo = typeof(ListAzureSearchMaterializer<T>).GetMethodInfo(f => f.Name == "Many" && f.IsStatic);

        readonly Func<T, object> projector;
        readonly Type elementType;

        /// <summary>
        /// Create an instance of the ListAzureSearchMaterializer with the given parameters.
        /// </summary>
        /// <param name="projector">A function to turn a hit into a desired CLR object.</param>
        /// <param name="elementType">The type of CLR object being materialized.</param>
        public ListAzureSearchMaterializer(Func<T, object> projector, Type elementType)
        {
            this.projector = projector;
            this.elementType = elementType;
        }

        /// <summary>
        /// Materialize the hits from the response into desired CLR objects.
        /// </summary>
        /// <param name="response">The <see cref="AzureOperationResponse"/> containing the hits to materialize.</param>
        /// <returns>List of <see cref="elementType"/> objects as constructed by the <see cref="projector"/>.</returns>
        public object Materialize(Response<SearchResults<T>> response)
        {
            Argument.EnsureNotNull(nameof(response), response);

            var responseBody = response.Value;

            var results = responseBody.GetResults();
            if (!results.Any())
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            }

            return manyMethodInfo
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { results.Select(q => q.Document), projector });
        }

        internal static IReadOnlyList<TTarget> Many<TTarget>(IEnumerable<T> documents, Func<T, object> projector)
        {
            return documents.Select(projector).Cast<TTarget>().ToReadOnlyBatchedList();
        }
    }
}
