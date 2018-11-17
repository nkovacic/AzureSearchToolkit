using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    public class CollectionTests
    {
        [Fact]
        public void CollectionContainsSingleConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.Tags.Contains("Bread - Dark Rye")));
        }

        [Fact]
        public void CollectionContainsSingleItem()
        {
            var item = "Bread - Dark Rye";

            DataAssert.Same<Listing>(q => q.Where(w => w.Tags.Contains(item)));
        }

        [Fact]
        public void CollectionNotContainsSingleConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => !w.Tags.Contains("Bread - Dark Rye")));
        }

        [Fact]
        public void CollectionNotContainsSingleItem()
        {
            var item = "Bread - Dark Rye";

            DataAssert.Same<Listing>(q => q.Where(w => !w.Tags.Contains(item)));
        }

        [Fact]
        public void CollectionAllSingleConstant()
        {
            Assert.Throws<NotSupportedException>(() => DataAssert.Data.SearchQuery<Listing>()
               .Where(w => AzureSearchMethods.ContainsAll(w.Tags, "Bread - Dark Rye")).ToList());
        }

        [Fact]
        public void CollectionAllSingleItem()
        {
            var item = "Bread - Dark Rye";

            Assert.Throws<NotSupportedException>(() => DataAssert.Data.SearchQuery<Listing>()
                .Where(w => AzureSearchMethods.ContainsAll(w.Tags, item)).ToList());
        }

        [Fact]
        public void CollectionNotAllSingleConstant()
        {
            DataAssert.SameSequence(DataAssert.Data.SearchQuery<Listing>().Where(w => !AzureSearchMethods.ContainsAll(w.Tags, "Bread - Dark Rye")).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags.All(q => !q.Contains("Bread - Dark Rye"))).ToList());
        }

        [Fact]
        public void CollectionNotAllContainsSingleItem()
        {
            var item = "Bread - Dark Rye";

            DataAssert.SameSequence(DataAssert.Data.SearchQuery<Listing>().Where(w => !AzureSearchMethods.ContainsAll(w.Tags, item)).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags.All(q => !q.Contains(item))).ToList());
        }


        [Fact]
        public void CollectionAnySingleConstant()
        {
            DataAssert.SameSequence(DataAssert.Data.SearchQuery<Listing>().Where(w => AzureSearchMethods.ContainsAny(w.Tags, "Bread - Dark Rye")).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags.Any(q => q.Contains("Bread - Dark Rye"))).ToList());
        }

        [Fact]
        public void CollectionAnyContainsSingleItem()
        {
            var item = "Bread - Dark Rye";

            DataAssert.SameSequence(DataAssert.Data.SearchQuery<Listing>().Where(w => AzureSearchMethods.ContainsAny(w.Tags, item)).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags.Any(q => q.Contains(item))).ToList());
        }


        [Fact]
        public void CollectionAnyContainsManyConstant()
        {
            DataAssert.SameSequence(
                DataAssert.Data.SearchQuery<Listing>().Where(w => AzureSearchMethods
                    .ContainsAny(w.Tags, new[] { "Bread - Dark Rye", "Beef - Striploin Aa" })).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags
                    .Any(q => new[] { "Bread - Dark Rye", "Beef - Striploin Aa" }.Contains(q))).ToList()
            );
        }

        [Fact]
        public void CollectionAnyContainsManyItems()
        {
            var items = new[] { "Bread - Dark Rye", "Beef - Striploin Aa" };

            DataAssert.SameSequence(DataAssert.Data.SearchQuery<Listing>().Where(w => AzureSearchMethods.ContainsAny(w.Tags, items)).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags.Any(q => items.Contains(q))).ToList());
        }

        [Fact]
        public void CollectionNotAllContainsManyConstant()
        {
            DataAssert.SameSequence(
                DataAssert.Data.SearchQuery<Listing>().Where(w => !AzureSearchMethods
                    .ContainsAll(w.Tags, new[] { "Bread - Dark Rye", "Beef - Striploin Aa" })).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags
                    .All(q => !new[] { "Bread - Dark Rye", "Beef - Striploin Aa" }.Contains(q))).ToList()
            );
        }

        [Fact]
        public void CollectionNotAllContainsManyItems()
        {
            var items = new[] { "Bread - Dark Rye", "Beef - Striploin Aa" };

            DataAssert.SameSequence(DataAssert.Data.SearchQuery<Listing>().Where(w => !AzureSearchMethods.ContainsAll(w.Tags, items)).ToList(),
                DataAssert.Data.Memory<Listing>().Where(w => w.Tags.All(q => !items.Contains(q))).ToList());
        }
    }
}
