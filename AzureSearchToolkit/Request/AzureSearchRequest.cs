using AzureSearchToolkit.Request.Criteria;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Request
{
    class AzureSearchRequest
    {
        public ICriteria Criteria { get; set; }
        public SearchParameters SearchParameters { get; }

        public AzureSearchRequest()
        {
            SearchParameters = new SearchParameters();
        }

        public void AddRangeToSelect(IEnumerable<string> fields)
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
    }
}
