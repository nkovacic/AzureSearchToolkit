using AzureSearch.Linq.Logging;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearch.Linq
{
    public interface IAzureSearchConnection
    {
        /// <summary>
        /// The name of the index on Azure Search instance.
        /// </summary>
        string Index { get; }

        /// <summary>
        /// How long to wait for a response to a network request before
        /// giving up.
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        /// Issues search requests to AzureSearch.
        /// </summary>
        /// <param name="searchParameters">Search parameters for the request.</param>
        /// <param name="searchText">The search text applied to the request.</param>
        /// <param name="logger">The logging mechanism for diagnostic information.</param>
        /// <returns>An AzureOperationResponse object containing the desired search results.</returns>
        Task<AzureOperationResponse<DocumentSearchResult<T>>> SearchAsync<T>(SearchParameters searchParameters, string searchText = null,
            ILogger logger = null) where T: class;
    }
}
