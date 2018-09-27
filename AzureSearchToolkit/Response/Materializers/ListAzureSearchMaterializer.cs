using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AzureSearchToolkit.Response.Materializers
{
    /// <summary>
    /// Materializes multiple hits into a list of CLR objects.
    /// </summary>
    class ListAzureSearchMaterializer : IAzureSearchMaterializer
    {
        static readonly MethodInfo manyMethodInfo = typeof(ListAzureSearchMaterializer).GetMethodInfo(f => f.Name == "Many" && f.IsStatic);

        readonly Func<Document, object> projector;
        readonly Type elementType;

        /// <summary>
        /// Create an instance of the ListAzureSearchMaterializer with the given parameters.
        /// </summary>
        /// <param name="projector">A function to turn a hit into a desired CLR object.</param>
        /// <param name="elementType">The type of CLR object being materialized.</param>
        public ListAzureSearchMaterializer(Func<Document, object> projector, Type elementType)
        {
            this.projector = projector;
            this.elementType = elementType;
        }

        /// <summary>
        /// Materialize the hits from the response into desired CLR objects.
        /// </summary>
        /// <param name="response">The <see cref="AzureOperationResponse"/> containing the hits to materialize.</param>
        /// <returns>List of <see cref="elementType"/> objects as constructed by the <see cref="projector"/>.</returns>
        public object Materialize(AzureOperationResponse<DocumentSearchResult> response)
        {
            Argument.EnsureNotNull(nameof(response), response);

            var responseBody = response.Body;

            if (responseBody?.Results == null || !responseBody.Results.Any())
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            }

            return manyMethodInfo
                .MakeGenericMethod(elementType)
                .Invoke(null, new object[] { responseBody.Results.Select(q => q.Document), projector });
        }

        internal static IReadOnlyList<T> Many<T>(IEnumerable<Document> documents, Func<Document, object> projector)
        {
            return documents.Select(projector).Cast<T>().ToReadOnlyBatchedList();
        }
    }
}
