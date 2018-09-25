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
    }
}
