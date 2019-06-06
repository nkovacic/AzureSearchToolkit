using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Response.Materializers
{
    /// <summary>
    /// Materializes true or false depending on whether any results matched the query or not.
    /// </summary>
    class AnyAzureSearchMaterializer : IAzureSearchMaterializer
    {
        /// <summary>
        /// Determine whether at a given query response contains any hits.
        /// </summary>
        /// <param name="response">The <see cref="AzureOperationResponse{T}"/> to check for emptiness.</param>
        /// <returns>true if the source sequence contains any elements; otherwise, false.</returns>
        public object Materialize(AzureOperationResponse<DocumentSearchResult<Document>> response)
        {
            if (response.Body.Count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(response), "Contains a negative number of hits.");
            }

            return response.Body.Count > 0;
        }
    }
}
