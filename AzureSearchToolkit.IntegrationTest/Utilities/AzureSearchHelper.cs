using AzureSearchToolkit.IntegrationTest.Configuration;
using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Request;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    class AzureSearchHelper: IDisposable
    {
        private string _searchName;

        private LamaConfiguration _configuration;
        private SearchServiceClient _serviceClient;

        private ILogger _logger { get; set; }

        public AzureSearchHelper(LamaConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;

            var searchKey = _configuration.GetModel().SearchKey;

            _searchName = _configuration.GetModel().SearchName;

            if (!string.IsNullOrWhiteSpace(_searchName) && !string.IsNullOrWhiteSpace(searchKey))
            {
                _serviceClient = new SearchServiceClient(_searchName, new SearchCredentials(searchKey));
            }
        }

        public async Task<ServiceResult<bool>> CreateSearchIndex<T>(string indexName = null) where T : class
        {
            var serviceResult = new ServiceResult<bool>();
            var indexExists = false;

            indexName = GetIndexName<T>(indexName);

            try
            {
                indexExists = await _serviceClient.Indexes.ExistsAsync(indexName);
            }
            catch (Exception e)
            {
                var message = $"Error on checking if {indexName} exists!";

                serviceResult.SetException(e);
                _logger.Log(TraceEventType.Error, e, null, message);

                return serviceResult.SetMessage(message);
            }

            if (indexExists)
            {
                serviceResult.Data = true;
            }
            else
            {
                var definition = new Index()
                {
                    Name = indexName,
                    Fields = FieldBuilder.BuildForType<T>()
                };

                try
                {
                    var result = await _serviceClient.Indexes.CreateAsync(definition);

                    if (result != null)
                    {
                        serviceResult.Data = true;
                    }
                }
                catch (Exception e)
                {
                    _logger.Log(TraceEventType.Error, e, null, "Index {0} was not created!", indexName);
                }
            }

            return serviceResult;
        }

        public async Task<ServiceResult<bool>> CreateDocumentInIndex<T>(T document, string indexName = null) where T : class
        {
            return await ChangeDocumentInIndex(document, indexName, AzureSearchIndexType.Upload);
        }

        public async Task<ServiceResult<bool>> CreateDocumentsInIndex<T>(IEnumerable<T> documents, string indexName = null) where T : class
        {
            return await ChangeDocumentsInIndex(documents, indexName, AzureSearchIndexType.Upload);
        }

        public async Task<ServiceResult<bool>> DeleteDocumentInIndex<T>(T document, string indexName = null) where T : class
        {
            return await ChangeDocumentInIndex(document, indexName, AzureSearchIndexType.Delete);
        }

        public async Task<ServiceResult<bool>> DeleteDocumentsInIndex<T>(IEnumerable<T> documents, string indexName = null) where T : class
        {
            return await ChangeDocumentsInIndex(documents, indexName, AzureSearchIndexType.Delete);
        }

        public async Task<ServiceResult<bool>> ChangeDocumentInIndex<T>(T document, string indexName = null,
            AzureSearchIndexType crudType = AzureSearchIndexType.MergeOrUpload) where T : class
        {
            return await ChangeDocumentsInIndex(new[] { document }, indexName, crudType);
        }

        public async Task<ServiceResult<bool>> ChangeDocumentsInIndex<T>(IEnumerable<T> documents, string indexName = null,
            AzureSearchIndexType crudType = AzureSearchIndexType.MergeOrUpload) where T : class
        {
            return await ChangeDocumentsInIndex(documents, new List<AzureSearchIndexType>() { crudType }, indexName);
        }

        public async Task<ServiceResult<bool>> ChangeDocumentsInIndex<T>(Dictionary<T, AzureSearchIndexType> documents, string indexName = null) where T : class
        {
            return await ChangeDocumentsInIndex(documents.Keys, documents.Values, indexName);
        }

        public async Task<ServiceResult<bool>> ChangeDocumentsInIndex<T>(IEnumerable<T> documents, IEnumerable<AzureSearchIndexType> crudTypes,
            string indexName = null) where T : class
        {
            var serviceResult = new ServiceResult<bool>();

            indexName = GetIndexName<T>(indexName);

            var searchIndexCreateServiceResult = await CreateSearchIndex<T>(indexName);

            if (!searchIndexCreateServiceResult.IsStatusOk())
            {
                return serviceResult.CopyStatus(searchIndexCreateServiceResult);
            }

            var indexActions = new List<IndexAction<T>>();

            var documentCounter = 0;

            foreach (var document in documents)
            {
                var crudType = AzureSearchIndexType.Upload;
                IndexAction<T> indexAction = null;

                if (crudTypes.Count() > documentCounter)
                {
                    crudType = crudTypes.ElementAt(documentCounter);
                }
                else
                {
                    crudType = crudTypes.First();
                }

                switch (crudType)
                {
                    case AzureSearchIndexType.Upload:
                        indexAction = IndexAction.Upload(document);
                        break;
                    case AzureSearchIndexType.Delete:
                        indexAction = IndexAction.Delete(document);
                        break;
                    case AzureSearchIndexType.Merge:
                        indexAction = IndexAction.Merge(document);
                        break;
                    default:
                        indexAction = IndexAction.MergeOrUpload(document);
                        break;
                }

                indexActions.Add(indexAction);

                documentCounter++;
            }

            var batch = IndexBatch.New(indexActions);
            var indexClient = _serviceClient.Indexes.GetClient(indexName);

            try
            {
                var documentIndexResult = await indexClient.Documents.IndexAsync(batch);

                serviceResult.Data = documentIndexResult.Results != null && documentIndexResult.Results.Count == documents.Count();
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                serviceResult.SetException(e);
                _logger.Log(TraceEventType.Error, e, null, "Failed to index some of the documents: {0}",
                    string.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
            }
            catch (Exception e)
            {
                serviceResult.SetException(e);
                _logger.Log(TraceEventType.Error, e, null, "Search index failed");
            }

            return serviceResult;
        }

        public async Task<ServiceResult<DocumentSearchResult<T>>> SearchDocuments<T>(SearchParameters searchParameters, string searchText = null,
            string indexName = null) where T : class
        {
            var serviceResult = new ServiceResult<DocumentSearchResult<T>>();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                searchText = "*";
            }

            indexName = GetIndexName<T>(indexName);

            var indexClient = _serviceClient.Indexes.GetClient(indexName);

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

                            _logger.Log(TraceEventType.Information, null,  new Dictionary<string, object>
                            {
                                {"SearchServiceName", _searchName },
                                {"SearchId", searchId},
                                {"IndexName", indexName},
                                {"QueryTerms", searchText}
                            }, "Search");
                        }

                        serviceResult.Data = response.Body;
                    }
                    else
                    {
                        serviceResult.SetStatusWithMessage(response.Response.StatusCode, $"Search failed for indexName {indexName}.");

                        _logger.Log(TraceEventType.Warning, null, null, $"Search failed for indexName {indexName}. Reason: {response.Response.ReasonPhrase}");
                    }
                }
                catch (Exception e)
                {
                    serviceResult
                        .SetException(e)
                        .SetMessage($"Search failed for indexName {indexName}.");
                    _logger.Log(TraceEventType.Error, e, null,
                        $"Search failed for indexName {indexName}. Query text: {searchText}, Query: {searchParameters.ToString()}");
                }
            }

            return serviceResult;
        }

        public async Task<ServiceResult<long>> CountDocuments<T>(SearchParameters searchParameters, string searchText = null,
            string indexName = null) where T : class
        {
            var serviceResult = new ServiceResult<long>();

            searchParameters.Top = 0;
            searchParameters.IncludeTotalResultCount = true;

            var documentSearchServiceResult = await SearchDocuments<T>(searchParameters, searchText, indexName);

            if (documentSearchServiceResult.IsStatusOk())
            {
                serviceResult.Data = documentSearchServiceResult.Data.Count.GetValueOrDefault();
            }
            else
            {
                serviceResult.CopyStatus(documentSearchServiceResult);
            }

            return serviceResult;
        }

        public SearchParameters GetSearchParameters(ApiParameters apiParameters)
        {
            var searchParameters = new SearchParameters
            {
                IncludeTotalResultCount = true,
                SearchMode = SearchMode.Any,
                Top = apiParameters.Limit,
                Skip = (apiParameters.Page - 1) * apiParameters.Limit,
                QueryType = QueryType.Full
            };

            if (apiParameters.IsSearchQuery())
            {
                searchParameters.SearchFields = apiParameters.GetSplittedQueryBy();
            }

            var orderBy = "";

            if (!string.IsNullOrWhiteSpace(apiParameters.OrderBy))
            {
                var validOrderByValues = new Dictionary<string, string>
                {
                    { "createdat", "createdAt" },
                    { "distance", "distance" },
                    { "popularity", "numberOfViews" },
                    { "price", "priceWithTaxAndDiscount" },
                    { "size", "overallSize" }
                };

                var orderByLowerCase = apiParameters.OrderBy.ToLowerInvariant();

                if (!validOrderByValues.ContainsKey(orderByLowerCase))
                {
                    orderBy = "createdAt desc";
                }
                else if (!orderByLowerCase.Equals("distance"))
                {
                    orderBy = validOrderByValues[orderByLowerCase] + " ";

                    if (orderByLowerCase.Equals("popularity"))
                    {
                        orderBy += "desc";
                    }
                    else
                    {
                        orderBy += (apiParameters.IsOrderDescending() ? "desc" : "asc");
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "createdAt desc";
            }

            searchParameters.OrderBy = new List<string> { orderBy };

            return searchParameters;
        }

        public string JoinFilters(string firstFilter, string secondFilter, bool isAnd = true)
        {
            return firstFilter + " " + (isAnd ? "and" : "or") + " " + secondFilter;
        }

        public string JoinFilters(IEnumerable<string> filters, bool isAnd = true)
        {
            var delimiter = isAnd ? "and" : "or";

            return string.Join(" " + delimiter + " ", filters);
        }

        public string GetEmptyFilter(string property, bool isEqual = true)
        {
            return GetFilter(property, null, null, isEqual);
        }

        public string GetFilter(string property, object value, bool? isMore = null, bool? isEqual = null)
        {
            var filter = property + " ";

            if (isMore.HasValue)
            {
                if (isEqual.GetValueOrDefault())
                {
                    filter += isMore.Value ? "ge" : "le";
                }
                else
                {
                    filter += isMore.Value ? "gt" : "lt";
                }
            }
            else if (isEqual.HasValue)
            {
                filter += isEqual.Value ? "eq" : "ne";
            }
            else
            {
                throw new ArgumentException("One of isMore or isEqual is required!");
            }

            var valueText = string.Empty;

            if (value == null)
            {
                valueText = "null";
            }
            else
            {
                valueText = value.ToString();

                if (value is string || value is Guid)
                {
                    valueText = "'" + valueText + "'";
                }
                else if (value is bool)
                {
                    valueText = value.ToString().ToLowerInvariant();
                }
                else if (value is DateTime)
                {
                    valueText = ((DateTime)value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else if (value is DateTimeOffset)
                {
                    valueText = ((DateTimeOffset)value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
            }

            return filter + " " + valueText;
        }

        public string GetFilter(IEnumerable<string> properties, object value, bool any = true, bool isEqual = true, bool? isMore = null)
        {
            var filter = string.Empty;

            if (value != null && properties.Any())
            {
                foreach (var property in properties)
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        if (any)
                        {
                            filter += " or ";
                        }
                        else
                        {
                            filter += " and ";
                        }
                    }

                    filter += GetFilter(property, value, isMore, isEqual);
                }

                filter = $"({filter})";
            }

            return filter;
        }

        public string GetFilterCollection(string property, string collectionElement, bool any = true, bool isEqual = true)
        {
            return $"{property}/{(any ? "any" : "all")}(t: t {(isEqual ? "eq" : "ne")} '{collectionElement}')";
        }

        public string[] GetFilterCollection(string property, IEnumerable<string> collection, bool any = true, bool isEqual = true)
        {
            var filters = new List<string>();

            if (collection.Count() == 1)
            {
                filters.Add(GetFilterCollection(property, collection.ElementAt(0), true, isEqual));
            }
            else
            {
                if (any || !isEqual)
                {
                    filters.Add($"{property}/{(any ? "any" : "all")}(t: search.in(t, '{string.Join(", ", collection)}'))");
                }
                else
                {
                    foreach (var item in collection)
                    {
                        filters.Add(GetFilterCollection(property, item, true, isEqual));
                        //filters.Add($"{property}/any(t: search.in(t, '{item}'))");
                    }
                }
            }

            return filters.ToArray();
        }

        public string GetFilterIn(string property, IEnumerable<string> values, bool any = true)
        {
            if (values.Any())
            {
                if (values.Count() == 1)
                {
                    return GetFilter(property, values.ElementAt(0), isEqual: true);
                }
                else
                {
                    return $"({string.Join(" or ", values.Select(q => $"{property} eq '{q}'"))})";
                }
                //return string.Format("search.in({0},'{1}')", property, string.Join(", ", values));
            }

            return string.Empty;
        }

        public string GetDistanceOrder(double latitude, double longitude, bool isAscending = true, string propertyName = "place")
        {
            return string.Format("geo.distance({0}, geography'POINT({1} {2})') {3}", propertyName,
                longitude.ToString().Replace(",", "."), latitude.ToString().Replace(",", "."), isAscending ? "asc" : "desc");
        }

        public string GetDistanceFilter(int maxDistanceInMeters, double latitude, double longitude, string propertyName = "place")
        {
            var lat = latitude.ToString().Replace(",", ".");
            var lng = longitude.ToString().Replace(",", ".");
            var distanceInKilometers = Math.Round((double)maxDistanceInMeters / 1000);

            return $"geo.distance({propertyName}, geography'POINT({lng} {lat})') le {distanceInKilometers}";
        }

        public string GetIntersectionFilter(double northEastCoordinateLatitude, double northEastCoordinateLongitude,
            double southWestCoordinateLatitude, double southWestCoordinateLongitude,
            string propertyName = "place")
        {
            var locations = new List<KeyValuePair<double, double>>()
            {
                new KeyValuePair<double, double>(northEastCoordinateLatitude, northEastCoordinateLongitude),
                new KeyValuePair<double, double>(northEastCoordinateLatitude, southWestCoordinateLongitude),
                new KeyValuePair<double, double>(southWestCoordinateLatitude, southWestCoordinateLongitude),
                new KeyValuePair<double, double>(southWestCoordinateLatitude, northEastCoordinateLongitude),
                new KeyValuePair<double, double>(northEastCoordinateLatitude, northEastCoordinateLongitude)
            };

            var filter = $"geo.intersects({propertyName}, geography'POLYGON((";

            foreach (var location in locations)
            {
                filter += $"{location.Value.ToString().Replace(",", ".")} {location.Key.ToString().Replace(",", ".")},";
            }

            filter = filter.TrimEnd(',');
            filter += "))')";

            return filter;
        }

        private string GetIndexName<T>(string indexName = null)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                var type = typeof(T);

                return GetIndexName($"{type.Name}s");
            }
            else
            {
                return GetIndexName(indexName);
            }
        }

        private string GetIndexName(string index)
        {
            return index;
        }

        public void Dispose()
        {
            _serviceClient.Dispose();
        }
    }
}
