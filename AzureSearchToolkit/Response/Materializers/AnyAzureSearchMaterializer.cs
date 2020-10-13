using Azure;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Response.Materializers
{
    /// <summary>
    /// Materializes true or false depending on whether any results matched the query or not.
    /// </summary>
    class AnyAzureSearchMaterializer<T> : IAzureSearchMaterializer<T>
    {
        /// <summary>
        /// Determine whether at a given query response contains any hits.
        /// </summary>
        /// <param name="response">The <see cref="AzureOperationResponse{T}"/> to check for emptiness.</param>
        /// <returns>true if the source sequence contains any elements; otherwise, false.</returns>
        public object Materialize(Response<SearchResults<T>> response)
        {
            if (response.Value.TotalCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(response), "Contains a negative number of hits.");
            }

            return response.Value.TotalCount > 0;
        }
    }
}
