using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    public class CountTests
    {
        static readonly Data data = new Data();

        static IQueryable<Listing> AzureListings { get; }

        static List<Listing> MemoryListings { get; }

        static DateTime? MidPoint { get; }

        static CountTests()
        {
            AzureListings = data.AzureSearch<Listing>();

            data.LoadToMemoryFromAzureSearch();

            MemoryListings = data.Memory<Listing>().ToList();

            MidPoint = MemoryListings.OrderBy(o => o.Id).Skip(MemoryListings.Count / 2).First().CreatedAt;
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
                data.AzureSearch<Listing>().Count(j => j.CreatedAt > MidPoint),
                data.Memory<Listing>().Count(j => j.CreatedAt > MidPoint));
        }

        [Fact]
        public void LongCountPredicate()
        {
            Assert.Equal(
                data.AzureSearch<Listing>().LongCount(j => j.CreatedAt > MidPoint),
                data.Memory<Listing>().LongCount(j => j.CreatedAt > MidPoint));
        }

        [Fact]
        public void WhereCount()
        {
            var midPoint = MemoryListings.OrderBy(o => o.CreatedAt).Skip(MemoryListings.Count / 2).First().CreatedAt;

            Assert.Equal(
                data.AzureSearch<Listing>().Where(j => j.CreatedAt > midPoint).Count(),
                data.Memory<Listing>().Where(j => j.CreatedAt > midPoint).Count());
        }

        [Fact]
        public void WhereLongCount()
        {
            var midPoint = MemoryListings.OrderBy(o => o.Id).Skip(MemoryListings.Count / 2).First().CreatedAt;

            Assert.Equal(
                data.AzureSearch<Listing>().Where(j => j.CreatedAt > midPoint).LongCount(),
                data.Memory<Listing>().Where(j => j.CreatedAt > midPoint).LongCount());
        }

        [Fact]
        public void QueryCount()
        {
            Assert.Equal(
                data.AzureSearch<Listing>().Where(j => j.Title.Contains("a")).Count(),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).Count());
        }

        [Fact]
        public void QueryLongCount()
        {
            Assert.Equal(
                data.AzureSearch<Listing>().Where(j => j.Title.Contains("a")).LongCount(),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).LongCount());
        }

        [Fact]
        public void QueryCountPredicate()
        {
            Assert.Equal(
                data.AzureSearch<Listing>().Where(j => j.Title.Contains("a")).Count(j => j.CreatedAt > MidPoint),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).Count(j => j.CreatedAt > MidPoint));
        }

        [Fact]
        public void QueryLongCountPredicate()
        {
            Assert.Equal(
                data.AzureSearch<Listing>().Where(j => j.Title.Contains("a")).LongCount(j => j.CreatedAt > MidPoint),
                data.Memory<Listing>().Where(j => j.Title.ToLowerInvariant().Contains("a")).LongCount(j => j.CreatedAt > MidPoint));
        }
    }
}
