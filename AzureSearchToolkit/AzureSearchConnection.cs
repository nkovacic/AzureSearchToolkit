using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Request;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchToolkit
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

        public AzureSearchConnection(string searchName, string searchKey, string index, TimeSpan? timeout = null)
        {
            if (timeout.HasValue)
            {
                Argument.EnsurePositive(nameof(timeout), timeout.Value);
            }
                
            Argument.EnsureNotBlank(nameof(index), index);
            Argument.EnsureNotBlank(nameof(searchName), searchName);
            Argument.EnsureNotBlank(nameof(searchKey), searchKey);

            SearchClient = new Lazy<SearchServiceClient>(() => new SearchServiceClient(searchName, new SearchCredentials(searchKey)));

            Index = index;
            Timeout = timeout ?? defaultTimeout;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<bool> ChangeDocumentsInIndexAsync<T>(Dictionary<T, IndexActionType> changedDocuments, ILogger logger = null) 
            where T : class
        {
            Argument.EnsureNotEmpty(nameof(changedDocuments), changedDocuments);

            if (logger == null)
            {
                logger = NullLogger.Instance;
            }

            await EnsureSearchIndexAsync<T>(logger);

            var index = Index;
            var indexActions = new List<IndexAction<T>>();

            foreach (var keyValuePair in changedDocuments)
            {
                IndexAction<T> indexAction = null;

                switch (keyValuePair.Value)
                {
                    case IndexActionType.Upload:
                        indexAction = IndexAction.Upload(keyValuePair.Key);
                        break;
                    case IndexActionType.Delete:
                        indexAction = IndexAction.Delete(keyValuePair.Key);
                        break;
                    case IndexActionType.Merge:
                        indexAction = IndexAction.Merge(keyValuePair.Key);
                        break;
                    default:
                        indexAction = IndexAction.MergeOrUpload(keyValuePair.Key);
                        break;
                }

                indexActions.Add(indexAction);
            }

            var batch = IndexBatch.New(indexActions);
            var indexClient = SearchClient.Value.Indexes.GetClient(index);

            try
            {
                var documentIndexResult = await indexClient.Documents.IndexAsync(batch);

                return documentIndexResult.Results != null && documentIndexResult.Results.Count == changedDocuments.Count();
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                logger.Log(TraceEventType.Error, e, null , "Failed to index some of the documents: {0}",
                    string.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
            }
            catch (Exception e)
            {
                logger.Log(TraceEventType.Error, e, null, "Search index failed");
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> EnsureSearchIndexAsync<T>(ILogger logger = null) where T : class
        {
            var index = Index;

            if (logger == null)
            {
                logger = NullLogger.Instance;
            }

            var indexExists = false;

            try
            {
                indexExists = await SearchClient.Value.Indexes.ExistsAsync(index);
            }
            catch (Exception e)
            {
                var message = $"Error on checking if {index} exists!";

                logger.Log(TraceEventType.Error, e, null, message);

                return false;
            }

            if (indexExists)
            {
                return false;
            }

            var definition = new Index()
            {
                Name = index,
                Fields = FieldBuilder.BuildForType<T>()
            };

            try
            {
                var result = await SearchClient.Value.Indexes.CreateAsync(definition);

                if (result != null)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Log(TraceEventType.Error, e, null, $"Index {index} was not created!", null);
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<AzureOperationResponse<DocumentSearchResult>> SearchAsync(SearchParameters searchParameters, string searchText = null,
            ILogger logger = null)
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
                    var response = await indexClient.Documents.SearchWithHttpMessagesAsync(searchText, searchParameters, customHeaders: headers);

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
                        logger.Log(TraceEventType.Warning, null, null, $"Search failed for indexName {Index}. Reason: {response.Response.ReasonPhrase}");
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
