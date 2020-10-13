using Azure;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchToolkit.Response.Materializers
{
    /// <summary>
    /// Interface for all materializers responsible for turning the AzureOperationResponse into desired
    /// CLR objects.
    /// </summary>
    interface IAzureSearchMaterializer<T>
    {
        /// <summary>
        /// Materialize the AzureOperationResponse into the desired CLR objects.
        /// </summary>
        /// <param name="response">The <see cref="AzureOperationResponse{T}"/> received from AzureSearch.</param>
        /// <returns>List or a single CLR object as requested.</returns>
        object Materialize(Response<SearchResults<T>> response);
    }
}
