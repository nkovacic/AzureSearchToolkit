using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class CountTests
    {
        static readonly Data data = new Data();

        static IQueryable<Listing> AzureListings { get; }

        static List<Listing> MemoryListings { get; }

        static double? MidPoint { get; }

        static CountTests()
        {
            data.LoadFromJsonToAzureSearch();
            data.LoadToMemoryFromAzureSearch();

            AzureListings = data.SearchQuery<Listing>();
            MemoryListings = data.Memory<Listing>().ToList();

            MidPoint = MemoryListings.OrderBy(o => o.Id).Skip(MemoryListings.Count / 2).First().Price;
        }

        [Fact]
        public void Count()
        {
            Assert.Equal(AzureListings.Count(), MemoryListings.Count());
        }

        [Fact]
        public void LongCount()
        {
            Assert.Equal(AzureListings.LongCount(), MemoryListings.LongCount());
        }

        [Fact]
        public void CountPredicate()
        {
            Assert.Equal(
                data.SearchQuery<Listing>().Count(j => j.Price > MidPoint),
                data.Memory<Listing>().Count(j => j.Price > MidPoint));
        }

        [Fact]
        public void LongCountPredicate()
        {
            Assert.Equal(
                data.SearchQuery<Listing>().LongCount(j => j.Price > MidPoint),
                data.Memory<Listing>().LongCount(j => j.Price > MidPoint));
        }

        [Fact]
        public void WhereCount()
        {
            var midPoint = MemoryListings.OrderBy(o => o.CreatedAt).Skip(MemoryListings.Count / 2).First().Price;

            Assert.Equal(
                data.SearchQuery<Listing>().Where(j => j.Price > midPoint).Count(),
                data.Memory<Listing>().Where(j => j.Price > midPoint).Count());
        }

        [Fact]
        public void WhereLongCount()
        {
            var midPoint = MemoryListings.OrderBy(o => o.Id).Skip(MemoryListings.Count / 2).First().Price;

            Assert.Equal(
                data.SearchQuery<Listing>().Where(j => j.Price > midPoint).LongCount(),
                data.Memory<Listing>().Where(j => j.Price > midPoint).LongCount());
        }

        [Fact]
        public void QueryCount()
        {
            Assert.Equal(
                data.SearchQuery<Listing>().Where(j => j.Title.Contains("a")).Count(),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).Count());
        }

        [Fact]
        public void QueryLongCount()
        {
            Assert.Equal(
                data.SearchQuery<Listing>().Where(j => j.Title.Contains("a")).LongCount(),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).LongCount());
        }

        [Fact]
        public void QueryCountPredicate()
        {
            Assert.Equal(
                data.SearchQuery<Listing>().Where(j => j.Title.Contains("a")).Count(j => j.Price > MidPoint),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).Count(j => j.Price > MidPoint));
        }

        [Fact]
        public void QueryLongCountPredicate()
        {
            Assert.Equal(
                data.SearchQuery<Listing>().Where(j => j.Title.Contains("a")).LongCount(j => j.Price > MidPoint),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).LongCount(j => j.Price > MidPoint));
        }
    }
}
