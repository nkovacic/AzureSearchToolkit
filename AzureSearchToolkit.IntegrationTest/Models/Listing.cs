using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Models
{
    [SerializePropertyNamesAsCamelCase]
    class Listing
    {
        [Key]
        [IsFilterable]
        public string Id { get; set; }

        [IsFacetable, IsSortable]
        public decimal Price { get; set; }

        [IsSearchable]
        public string Title { get; set; }

        [IsFilterable]
        public bool Active { get; set; }

        [IsFilterable]
        public string[] Tags { get; set; }
    }
}
