using AzureSearchToolkit.IntegrationTest.Configuration;
using AzureSearchToolkit.IntegrationTest.Models;
using AzureSearchToolkit.IntegrationTest.Utilities;
using AzureSearchToolkit.Logging;
using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Request;
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
        const string Index = "integration-test";

        static readonly AzureSearchConnection azureSearchConnection;
        static readonly AzureSearchContext azureSearchContext;

        readonly List<object> memory = new List<object>();

        static Data()
        {
            azureSearchConnection = new AzureSearchConnection(LamaConfiguration.Current().GetModel().SearchName,
                LamaConfiguration.Current().GetModel().SearchKey, Index);

            azureSearchContext = new AzureSearchContext(azureSearchConnection, new AzureSearchMapping());
        }

        public IQueryable<T> AzureSearch<T>()
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

            const int expectedDataCount = 100;

            if (memory.Count != expectedDataCount)
            {
                throw new InvalidOperationException(
                    $"Tests expect {expectedDataCount} entries but {memory.Count} loaded from AzureSearch index '{Index}'");
            }              
        }

        public void LoadFromJsonToAzureSearch()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var mockedDataPath = Path.Combine(baseDirectory, "App_Data/listings_mocked.json");

            var listings = JsonConvert.DeserializeObject<List<Listing>>(File.ReadAllText(mockedDataPath));

            using (var azureSearchHelper = new AzureSearchHelper(LamaConfiguration.Current(), NullLogger.Instance))
            {
                var createIndexServiceResult = AsyncHelper.RunSync(() => azureSearchHelper.CreateSearchIndex<Listing>(Index));

                if (!createIndexServiceResult.IsStatusOk())
                {
                    throw createIndexServiceResult.PotentialException.GetException();
                }

                var uploadOrMergeListingsServiceResult = AsyncHelper.RunSync(() => azureSearchHelper.ChangeDocumentsInIndex(listings));

                if (!uploadOrMergeListingsServiceResult.IsStatusOk())
                {
                    throw uploadOrMergeListingsServiceResult.PotentialException.GetException();
                }
            }
        }
    }
}
