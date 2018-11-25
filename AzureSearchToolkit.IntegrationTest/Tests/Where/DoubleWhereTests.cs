using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class DoubleWhereTests
    {
        static double MiddlePrice;

        static DoubleWhereTests()
        {
            MiddlePrice = DataAssert.Data.Memory<Listing>().Skip(DataAssert.Data.Memory<Listing>().Count() / 2).First().Price;
        }

        [Fact]
        public void LessThanConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.Price < MiddlePrice));
            DataAssert.Same<Listing>(q => q.Where(w => MiddlePrice < w.Price));
        }

        [Fact]
        public void LessThanOrEqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.Price <= MiddlePrice));
            DataAssert.Same<Listing>(q => q.Where(w => MiddlePrice <= w.Price));
        }

        [Fact]
        public void GreaterThanConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.Price > MiddlePrice));
            DataAssert.Same<Listing>(q => q.Where(w => MiddlePrice > w.Price));
        }

        [Fact]
        public void GreaterThanOrEqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.Price >= MiddlePrice));
            DataAssert.Same<Listing>(q => q.Where(w => MiddlePrice >= w.Price));
        }

        [Fact]
        public void EqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => MiddlePrice == w.Price));
            DataAssert.Same<Listing>(q => q.Where(w => w.Price == MiddlePrice));
            DataAssert.Same<Listing>(q => q.Where(w => w.Price.Equals(MiddlePrice)));
            DataAssert.Same<Listing>(q => q.Where(w => MiddlePrice.Equals(w.Price)));
            DataAssert.Same<Listing>(q => q.Where(w => Equals(w.Price, MiddlePrice)));
            DataAssert.Same<Listing>(q => q.Where(w => Equals(MiddlePrice, w.Price)));
        }

        [Fact]
        public void NotEqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => MiddlePrice != w.Price));
            DataAssert.Same<Listing>(q => q.Where(w => w.Price != MiddlePrice));
            DataAssert.Same<Listing>(q => q.Where(w => !w.Price.Equals(MiddlePrice)));
            DataAssert.Same<Listing>(q => q.Where(w => !MiddlePrice.Equals(w.Price)));
            DataAssert.Same<Listing>(q => q.Where(w => !Equals(w.Price, MiddlePrice)));
            DataAssert.Same<Listing>(q => q.Where(w => !Equals(MiddlePrice, w.Price)));
        }
    }
}
