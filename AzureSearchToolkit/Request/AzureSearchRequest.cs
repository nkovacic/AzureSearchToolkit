using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using AzureSearchToolkit.Request.Criteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit.Request
{
    class AzureSearchRequest
    {
        public string SearchText { get; set; }
        public ICriteria Criteria { get; set; }
        public SearchOptions SearchOptions { get; }

        public AzureSearchRequest()
        {
            SearchOptions = new SearchOptions
            {
                QueryType = SearchQueryType.Simple,
                SearchMode = SearchMode.All,
                Size = 200
            };
            SearchText = "*";
        }

        public void AddOrderByField(string orderByField, bool addToStart = true)
        {
            AddOrderByFields(new[] { orderByField }, addToStart);
        }

        public void AddOrderByFields(string[] orderByFields, bool addToStart = true)
        {
            if (addToStart)
            {
                foreach (var field in orderByFields.Reverse())
                {
                    SearchOptions.OrderBy.Insert(0, field);
                }
            }
            else
            {
                foreach (var field in orderByFields)
                {
                    SearchOptions.OrderBy.Add(field);
                }
            }
        }

        public void AddRangeToSelect(params string[] fields)
        {
            foreach (var field in fields)
            {
                SearchOptions.Select.Add(field);
            }
        }

        public void AddRangeToSearchFields(params string[] searchFields)
        {
            foreach (var field in searchFields)
            {
                SearchOptions.SearchFields.Add(field);
            }
        }
    }
}
