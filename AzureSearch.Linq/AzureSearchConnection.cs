using AzureSearch.Linq.Logging;
using AzureSearch.Linq.Utlities;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearch.Linq
{
    public class AzureSearchConnection: IAzureSearchConnection, IDisposable
    {
        static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(10);

        internal Lazy<SearchServiceClient> SearchClient { get; private set; }

        /// <summary>
        /// The name of the index on the AzureSearch instance.
        /// </summary>
        /// <example>northwind</example>
        public string Index { get; }

        /// <summary>
        /// How long to wait for a response to a network request before
        /// giving up.
        /// </summary>
        public TimeSpan Timeout { get; }

        public AzureSearchConnection(string searchName, string searchKey, string index = null, TimeSpan? timeout)
        {
            if (timeout.HasValue)
            {
                Argument.EnsurePositive(nameof(timeout), timeout.Value);
            }
                
            if (index != null)
            {
                Argument.EnsureNotBlank(nameof(index), index);
            }

            Argument.EnsureNotBlank(nameof(searchName), searchName);
            Argument.EnsureNotBlank(nameof(searchKey), searchKey);

            SearchClient = new Lazy<SearchServiceClient>(() => new SearchServiceClient(searchName, new SearchCredentials(searchKey)));

            Index = index;
            Timeout = timeout ?? defaultTimeout;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (SearchClient.IsValueCreated)
                {
                    SearchClient.Value.Dispose();
                }
            }
        }

        public async Task<AzureOperationResponse<DocumentSearchResult<T>>> SearchAsync<T>(SearchParameters searchParameters, string searchText = null,
            ILogger logger = null) where T: class
        {
            if (logger == null)
            {
                logger = NullLogger.Instance;
            }

            var indexClient = SearchClient.Value.Indexes.GetClient(Index);

            if (indexClient != null)
            {
                var headers = new Dictionary<string, List<string>>() { { "x-ms-azs-return-searchid", new List<string>() { "true" } } };

                try
                {
                    var response = await indexClient.Documents.SearchWithHttpMessagesAsync<T>(searchText, searchParameters, customHeaders: headers);

                    if (response.Response.IsSuccessStatusCode)
                    {
                        IEnumerable<string> headerValues = null;

                        if (response.Response.Headers.TryGetValues("x-ms-azs-searchid", out headerValues))
                        {
                            var searchId = headerValues.FirstOrDefault();

                            logger.Log(TraceEventType.Information, null, new Dictionary<string, object>
                            {
                                {"SearchServiceName", SearchClient.Value.SearchServiceName },
                                {"SearchId", searchId},
                                {"IndexName", Index},
                                {"QueryTerms", searchText}
                            }, "Search");
                        }
                    }
                    else
                    {
                        logger.Log(TraceEventType.Warning, null, null,  $"Search failed for indexName {Index}. Reason: {response.Response.ReasonPhrase}");
                    }

                    return response;
                }
                catch (Exception e)
                {
                    logger.Log(TraceEventType.Error, e, null, $"Search failed for indexName {Index}. Query text: {searchText}, Query: {searchParameters.ToString()}, Reason: {e.Message}");

                    throw e;
                }
            }
            else
            {
                logger.Log(TraceEventType.Warning, null, null, $"Problem with creating search client for {Index} index!");
            }

            return null;
        }
    }
}
