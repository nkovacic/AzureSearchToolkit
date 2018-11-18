using AzureSearchToolkit.IntegrationTest.Models;
using System.Linq;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class OrderTests
    {
        [Fact]
        public void OrderByDouble()
        {
            DataAssert.Same<Listing>(q => q.OrderBy(w => w.Price), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByString()
        {
            DataAssert.Same<Listing>(q => q.OrderBy(w => w.Title), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByDateTime()
        {
            DataAssert.Same<Listing>(q => q.OrderBy(w => w.CreatedAt), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByStringThenByDateTime()
        {
            DataAssert.Same<Listing>(q => q.OrderBy(w => w.Title).ThenBy(w => w.CreatedAt), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByStringThenByDateTimeDescending()
        {
            DataAssert.Same<Listing>(q => q.OrderBy(w => w.Title).ThenByDescending(w => w.CreatedAt), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByDoubleDescending()
        {
            DataAssert.Same<Listing>(q => q.OrderByDescending(w => w.Price), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByStringDescending()
        {
            DataAssert.Same<Listing>(q => q.OrderByDescending(w => w.Title), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByDateTimeDescending()
        {
            DataAssert.Same<Listing>(q => q.OrderByDescending(w => w.CreatedAt), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByStringDescendingThenByDateTime()
        {
            DataAssert.Same<Listing>(q => q.OrderByDescending(w => w.Title).ThenBy(w => w.CreatedAt), useDefaultOrder: false);
        }

        [Fact]
        public void OrderByStringDescendingThenByDateTimeDescending()
        {
            DataAssert.Same<Listing>(q => q.OrderByDescending(w => w.Title).ThenByDescending(w => w.CreatedAt), useDefaultOrder: false);
        }
    }
}