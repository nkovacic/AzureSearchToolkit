using AzureSearchToolkit.IntegrationTest.Configuration;
using AzureSearchToolkit.IntegrationTest.Models;
using AzureSearchToolkit.IntegrationTest.Utilities;
using AzureSearchToolkit.Json;
using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Request;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest
{
    class Data
    {
        public const string Index = "azure-search-toolkit-integration-test";

        const int ExpectedDataCount = 100;
         
        static readonly AzureSearchConnection azureSearchConnection;
        static readonly AzureSearchContext azureSearchContext;

        readonly List<object> memory = new List<object>();

        static Data()
        {
            azureSearchConnection = new AzureSearchConnection(LamaConfiguration.Current().GetModel().SearchName,
                LamaConfiguration.Current().GetModel().SearchKey);

            azureSearchContext = new AzureSearchContext(azureSearchConnection, new AzureSearchMapping());
        }

        public AzureSearchContext SearchContext()
        {
            return azureSearchContext;
        }

        public IQueryable<T> SearchQuery<T>() where T : class
        {
            return azureSearchContext.Query<T>();
        }

        public IQueryable<T> Memory<T>()
        {
            return memory.AsQueryable().OfType<T>();
        }

        public void LoadToMemoryFromAzureSearch()
        {
            memory.Clear();
            memory.AddRange(azureSearchContext.Query<Listing>());

            if (memory.Count != ExpectedDataCount)
            {
                throw new InvalidOperationException(
                    $"Tests expect {ExpectedDataCount} entries but {memory.Count} loaded from AzureSearch index '{Index}'");
            }              
        }

        public void LoadFromJsonToAzureSearch()
        {
            using (var azureSearchHelper = new AzureSearchHelper(LamaConfiguration.Current(), NullLogger.Instance))
            {
                var createIndexServiceResult = AsyncHelper.RunSync(() => azureSearchHelper.CreateSearchIndex<Listing>(Index));

                if (!createIndexServiceResult.IsStatusOk())
                {
                    throw createIndexServiceResult.PotentialException.GetException();
                }

                var countServiceResult = AsyncHelper.RunSync(() => azureSearchHelper.CountDocuments<Listing>(new SearchParameters(), indexName: Index));

                if (!countServiceResult.IsStatusOk() || countServiceResult.Data != ExpectedDataCount)
                {
                    var baseDirectory = AppContext.BaseDirectory;
                    var mockedDataPath = Path.Combine(baseDirectory, "App_Data\\listings-mocked.json");

                    var searchParameters = new SearchParameters()
                    {
                        Select = new List<string>() { "id" },
                        Top = 1000
                    };

                    if (countServiceResult.Data > 0)
                    {
                        var allDocuments = AsyncHelper.RunSync(() => azureSearchHelper.SearchDocuments<Listing>(searchParameters, indexName: Index));

                        AsyncHelper.RunSync(() => azureSearchHelper
                            .DeleteDocumentsInIndex(allDocuments.Data.Results.Select(q => new Listing { Id = q.Document.Id }), Index));

                        azureSearchHelper.WaitForSearchOperationCompletion<Listing>(0, Index);
                    }
                    
                    var listings = JsonConvert.DeserializeObject<List<Listing>>(File.ReadAllText(mockedDataPath), new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new GeographyPointJsonConverter() }
                    });

                    var uploadOrMergeListingsServiceResult = AsyncHelper.RunSync(() => azureSearchHelper.ChangeDocumentsInIndex(listings, Index));

                    if (!uploadOrMergeListingsServiceResult.IsStatusOk())
                    {
                        throw uploadOrMergeListingsServiceResult.PotentialException.GetException();
                    }

                    azureSearchHelper.WaitForSearchOperationCompletion<Listing>(listings.Count, Index);
                }
            }
        }

        public void WaitForSearchOperationCompletion(int numberOfRequiredItemsInSearch)
        {
            using (var azureSearchHelper = new AzureSearchHelper(LamaConfiguration.Current(), NullLogger.Instance))
            {
                azureSearchHelper.WaitForSearchOperationCompletion<Listing>(numberOfRequiredItemsInSearch, Index);
            }
        }
    }
}
