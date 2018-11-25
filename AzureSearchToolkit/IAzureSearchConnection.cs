using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Request;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchToolkit
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
        /// Create index if it does not exists
        /// </summary>
        /// <param name="index">Optionally override default index</param>
        /// <returns>If the index was created, true is returned, otherwise false</returns>
        Task<bool> EnsureSearchIndexAsync<T>(ILogger logger = null) where T : class;

        /// <summary>
        /// Change documents in index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="changedDocuments">Dictionary with documents to change. Included are AzureSearchIndexType operation for each document.</param>
        /// <returns>If documents were succesfully changed, true is returned, otherwise false</returns>
        Task<bool> ChangeDocumentsInIndexAsync<T>(Dictionary<T, IndexActionType> changedDocuments, ILogger logger = null) where T : class;

        /// <summary>
        /// Issues search requests to AzureSearch.
        /// </summary>
        /// <param name="searchParameters">Search parameters for the request.</param>
        /// <param name="searchText">The search text applied to the request.</param>
        /// <param name="logger">The logging mechanism for diagnostic information.</param>
        /// <returns>An AzureOperationResponse object containing the desired search results.</returns>
        Task<AzureOperationResponse<DocumentSearchResult>> SearchAsync(SearchParameters searchParameters, string searchText = null,
            ILogger logger = null);
    }
}
