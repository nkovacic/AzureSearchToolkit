using AzureSearchToolkit.Attributes;
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
    public class AzureSearchConnection : IAzureSearchConnection, IDisposable
    {
        static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Placeholder for index when no Type is used in constructor
        /// </summary>
        string IndexPlaceholder { get; set; }

        /// <summary>
        /// Azure Search index mapping for CLR Type
        /// </summary>
        readonly Dictionary<Type, string> indexes;

        internal Lazy<SearchServiceClient> SearchClient { get; private set; }

        /// <summary>
        /// How long to wait for a response to a network request before
        /// giving up.
        /// </summary>
        public TimeSpan Timeout { get; }

        public AzureSearchConnection(string searchName, string searchKey, TimeSpan? timeout = null)
            : this(searchName, searchKey, string.Empty, timeout) { }

        public AzureSearchConnection(string searchName, string searchKey, string index, TimeSpan? timeout = null)
        {
            if (timeout.HasValue)
            {
                Argument.EnsurePositive(nameof(timeout), timeout.Value);
            }

            Argument.EnsureNotBlank(nameof(searchName), searchName);
            Argument.EnsureNotBlank(nameof(searchKey), searchKey);

            IndexPlaceholder = index;
            indexes = new Dictionary<Type, string>();

            SearchClient = new Lazy<SearchServiceClient>(() => new SearchServiceClient(searchName, new SearchCredentials(searchKey)));
            Timeout = timeout ?? defaultTimeout;
        }

        public AzureSearchConnection(string searchName, string searchKey, string index, Type indexType, TimeSpan? timeout = null)
            : this(searchName, searchKey, new Dictionary<Type, string>() { { indexType, index } }, timeout) { }

        public AzureSearchConnection(string searchName, string searchKey, Dictionary<Type, string> indexesWithType, TimeSpan? timeout = null)
        {
            if (timeout.HasValue)
            {
                Argument.EnsurePositive(nameof(timeout), timeout.Value);
            }

            Argument.EnsureNotEmpty(nameof(indexesWithType), indexesWithType);
            Argument.EnsureNotBlank(nameof(searchName), searchName);
            Argument.EnsureNotBlank(nameof(searchKey), searchKey);

            foreach (var indexWithType in indexesWithType)
            {
                Argument.EnsureNotNull(nameof(indexWithType.Key), indexWithType.Key);
                Argument.EnsureNotBlank(nameof(indexWithType.Value), indexWithType.Value);
            }

            if (indexes.GroupBy(q => q.Key).Count() != indexes.Count)
            {
                throw new ArgumentException("Duplicate types found in indexes!");
            }

            if (indexes.GroupBy(q => q.Value).Count() != indexes.Count)
            {
                throw new ArgumentException("Duplicate index names found in indexes!");
            }

            indexes = indexesWithType;

            SearchClient = new Lazy<SearchServiceClient>(() => new SearchServiceClient(searchName, new SearchCredentials(searchKey)));
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
        public async Task<bool> ChangeDocumentsInIndexAsync<T>(SortedDictionary<T, IndexActionType> changedDocuments, ILogger logger = null)
            where T : class
        {
            Argument.EnsureNotEmpty(nameof(changedDocuments), changedDocuments);

            if (logger == null)
            {
                logger = NullLogger.Instance;
            }

            await EnsureSearchIndexAsync<T>(logger);

            var index = GetIndex<T>();
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
                logger.Log(TraceEventType.Error, e, null, "Failed to index some of the documents: {0}",
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
            var index = GetIndex<T>();

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
        public async Task<AzureOperationResponse<DocumentSearchResult<Document>>> SearchAsync(SearchParameters searchParameters, Type searchType,
            string searchText = null, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = NullLogger.Instance;
            }

            var searchIndex = GetIndex(searchType);
            var indexClient = SearchClient.Value.Indexes.GetClient(searchIndex);

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
                                {"IndexName", searchIndex},
                                {"QueryTerms", searchText}
                            }, "Search");
                        }
                    }
                    else
                    {
                        logger.Log(TraceEventType.Warning, null, null,
                            $"Search failed for indexName {searchIndex}. Reason: {response.Response.ReasonPhrase}");
                    }

                    return response;
                }
                catch (Exception e)
                {
                    logger.Log(TraceEventType.Error, e, null,
                        $"Search failed for indexName {searchIndex}. Query text: {searchText}, Query: {searchParameters.ToString()}, Reason: {e.Message}");

                    throw e;
                }
            }
            else
            {
                logger.Log(TraceEventType.Warning, null, null, $"Problem with creating search client for {searchIndex} index!");
            }

            return null;
        }

        private string GetIndex<T>() where T : class
        {
            return GetIndex(typeof(T));
        }

        private string GetIndex(Type type)
        {
            if (!indexes.ContainsKey(type))
            {
                if (!string.IsNullOrWhiteSpace(IndexPlaceholder) && indexes.Count == 0)
                {
                    indexes.Add(type, IndexPlaceholder);

                    IndexPlaceholder = null;
                }
                else
                {
                    var searchIndex = TypeHelper.GetAttributeValue<AzureSearchIndexAttribute, string>(type, (searchAttribute) => searchAttribute.Index);

                    if (!string.IsNullOrWhiteSpace(searchIndex))
                    {
                        indexes.Add(type, searchIndex);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"AzureSearch index for type {type} was not found!");
                    }
                }
            }

            return indexes[type];
        }
    }
}
