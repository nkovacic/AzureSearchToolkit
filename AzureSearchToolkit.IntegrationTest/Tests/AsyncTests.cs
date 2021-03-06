﻿using AzureSearchToolkit.IntegrationTest.Models;
using AzureSearchToolkit.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class AsyncTests
    {
        static double MiddlePrice;

        static AsyncTests()
        {
            MiddlePrice = DataAssert.Data.Memory<Listing>().Skip(DataAssert.Data.Memory<Listing>().Count() / 2).First().Price + 1;
        }

        [Fact]
        public async void ToListAsyncReturnsCorrectResults()
        {
            var memory = DataAssert.Data.Memory<Listing>().Where(w => w.Price > MiddlePrice).OrderBy(w => w.Price).ToList();
            var search = await DataAssert.Data.SearchQuery<Listing>().Where(w => w.Price > MiddlePrice).OrderBy(w => w.Price).ToListAsync();

            DataAssert.SameSequence(memory, search);
        }

        /*
        [Fact]
        public async void ToDictionaryAsyncReturnsCorrectResults()
        {
            var memory = DataAssert.Data.Memory<Listing>().Where(w => w.Price > MiddlePrice).ToDictionary(k => k.Id);
            var search = await DataAssert.Data.AzureSearch<Listing>().Where(w => w.Price > MiddlePrice).ToDictionaryAsync(k => k.Id);

            Assert.Equal(memory.Count, search.Count);

            foreach (var kvp in memory)
                Assert.Equal(search[kvp.Key], kvp.Value);
        }
        */
    
        [Fact]
        public async void FirstAsyncReturnsCorrectResult()
        {
            var memory = DataAssert.Data.Memory<Listing>().Where(w => w.Price > MiddlePrice).OrderBy(q => q.CreatedAt).First();
            var search = await DataAssert.Data.SearchQuery<Listing>().Where(w => w.Price > MiddlePrice).OrderBy(q => q.CreatedAt).FirstAsync();

            Assert.Equal(memory, search);
        }

        [Fact]
        public async void FirstOrDefaultAsyncWithPredicateReturnsCorrectResult()
        {
            var memory = DataAssert.Data.Memory<Listing>().OrderBy(q => q.CreatedAt).FirstOrDefault(w => w.Price == MiddlePrice);
            var search = await DataAssert.Data.SearchQuery<Listing>().OrderBy(q => q.CreatedAt).FirstOrDefaultAsync(w => w.Price == MiddlePrice);

            Assert.Equal(memory, search);
        }
    }
}
