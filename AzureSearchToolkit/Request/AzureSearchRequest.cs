using AzureSearchToolkit.Request.Criteria;
using Microsoft.Azure.Search.Models;
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
        public SearchParameters SearchParameters { get; }

        public AzureSearchRequest()
        {
            SearchParameters = new SearchParameters
            {
                QueryType = QueryType.Simple,
                SearchMode = SearchMode.All,
                Top = 200
            };
            SearchText = "*";
        }

        public void AddOrderByField(string orderByField, bool addToStart = true)
        {
            AddOrderByFields(new[] { orderByField }, addToStart);
        }

        public void AddOrderByFields(string[] orderByFields, bool addToStart = true)
        {
            if (SearchParameters.OrderBy == null)
            {
                SearchParameters.OrderBy = new List<string>();
            }

            if (addToStart)
            {
                foreach (var field in orderByFields.Reverse())
                {
                    SearchParameters.OrderBy.Insert(0, field);
                }
            }
            else
            {
                foreach (var field in orderByFields)
                {
                    SearchParameters.OrderBy.Add(field);
                }
            }
        }

        public void AddRangeToSelect(params string[] fields)
        {
            if (SearchParameters.Select == null)
            {
                SearchParameters.Select = new List<string>();
            }

            foreach (var field in fields)
            {
                SearchParameters.Select.Add(field);
            }
        }

        public void AddRangeToSearchFields(params string[] searchFields)
        {
            if (SearchParameters.Select == null)
            {
                SearchParameters.SearchFields = new List<string>();
            }

            foreach (var field in searchFields)
            {
                SearchParameters.SearchFields.Add(field);
            }
        }
    }
}
