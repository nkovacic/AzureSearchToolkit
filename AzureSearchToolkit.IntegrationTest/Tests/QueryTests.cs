using AzureSearchToolkit.IntegrationTest.Models;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class QueryTests
    {
        [Fact]
        public void LuceneQueryRegex()
        {
            var item = "Smirnoff";

            DataAssert.Same(DataAssert.Data.SearchQuery<Listing>().LuceneQuery($"/.*{item}.*/").OrderBy(q => q.CreatedAt),
                DataAssert.Data.Memory<Listing>()
                    .Where(w => w.Title.Contains(item, StringComparison.OrdinalIgnoreCase)
                        || w.Description.Contains(item, StringComparison.OrdinalIgnoreCase)).OrderBy(q => q.CreatedAt));
        }

        [Fact]
        public void SimpleQuerySingleItem()
        {
            var item = "Vodka - Smirnoff";

            DataAssert.Same(DataAssert.Data.SearchQuery<Listing>().SimpleQuery($"'{item}'").OrderBy(q => q.CreatedAt),
                DataAssert.Data.Memory<Listing>()
                    .Where(w => w.Title.Contains(item, StringComparison.OrdinalIgnoreCase)
                        || w.Description.Contains(item, StringComparison.OrdinalIgnoreCase)).OrderBy(q => q.CreatedAt));
        }

        [Fact]
        public void SimpleQueryMultiItemAnd()
        {
            var items = new[] { "Vestibulum", "aenean" };

            DataAssert.Same(DataAssert.Data.SearchQuery<Listing>().SimpleQuery(string.Join('+', items)).OrderBy(q => q.CreatedAt),
                DataAssert.Data.Memory<Listing>()
                    .Where(w => items.All(item => w.Description.Contains(item, StringComparison.OrdinalIgnoreCase))
                        || items.All(item => w.Title.Contains(item, StringComparison.OrdinalIgnoreCase))).OrderBy(q => q.CreatedAt));
        }

        [Fact]
        public void SimpleQueryMultiItemAny()
        {
            var items = new[] { "Vestibulum", "aenean" };

            DataAssert.Same(DataAssert.Data.SearchQuery<Listing>().SimpleQuery(string.Join('|', items)).OrderBy(q => q.CreatedAt),
                DataAssert.Data.Memory<Listing>()
                    .Where(w => items.Any(item => w.Description.Contains(item, StringComparison.OrdinalIgnoreCase))
                        || items.Any(item => w.Title.Contains(item, StringComparison.OrdinalIgnoreCase))).OrderBy(q => q.CreatedAt));
        }

        [Fact]
        public void SimpleQueryMultiItemModeAnd()
        {
            var items = new[] { "Vestibulum", "aenean" };

            DataAssert.Same(DataAssert.Data.SearchQuery<Listing>().SimpleQuery(string.Join(' ', items), SearchMode.All).OrderBy(q => q.CreatedAt),
                DataAssert.Data.Memory<Listing>()
                    .Where(w => items.All(item => w.Description.Contains(item, StringComparison.OrdinalIgnoreCase))
                        || items.All(item => w.Title.Contains(item, StringComparison.OrdinalIgnoreCase))).OrderBy(q => q.CreatedAt));
        }

        [Fact]
        public void SimpleQueryMultiItemOnlyTitle()
        {
            var items = new[] { "Vestibulum", "aenean" };

            DataAssert.Same(DataAssert.Data.SearchQuery<Listing>()
                .SimpleQuery(string.Join(' ', items), searchFields: q => q.Title).OrderBy(q => q.CreatedAt),
                DataAssert.Data.Memory<Listing>()
                    .Where(w => items.All(item => w.Title.Contains(item, StringComparison.OrdinalIgnoreCase))).OrderBy(q => q.CreatedAt));
        }
    }
}
