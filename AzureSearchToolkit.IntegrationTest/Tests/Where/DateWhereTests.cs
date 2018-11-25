using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureSearchToolkit.IntegrationTest.Models;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class DateWhereTests
    {
        readonly DateTime averageCreatedDate = new DateTime(2018, 4, 1);

        [Fact]
        public void LessThanConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.CreatedAt < averageCreatedDate));
        }

        [Fact]
        public void LessThanOrEqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.CreatedAt <= averageCreatedDate));
            DataAssert.Same<Listing>(q => q.Where(w => averageCreatedDate <= w.CreatedAt));
        }

        [Fact]
        public void GreaterThanConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.CreatedAt > averageCreatedDate));
            DataAssert.Same<Listing>(q => q.Where(w => averageCreatedDate > w.CreatedAt));
        }

        [Fact]
        public void GreaterThanOrEqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => w.CreatedAt >= averageCreatedDate));
            DataAssert.Same<Listing>(q => q.Where(w => averageCreatedDate >= w.CreatedAt));
        }

        [Fact]
        public void EqualToConstant()
        {
            var firstDate = DataAssert.Data.Memory<Listing>().First().CreatedAt;

            DataAssert.Same<Listing>(q => q.Where(w => firstDate == w.CreatedAt));
            DataAssert.Same<Listing>(q => q.Where(w => w.CreatedAt == firstDate));
            DataAssert.Same<Listing>(q => q.Where(w => w.CreatedAt.Equals(firstDate)));
            DataAssert.Same<Listing>(q => q.Where(w => firstDate.Equals(w.CreatedAt)));
            DataAssert.Same<Listing>(q => q.Where(w => Equals(w.CreatedAt, firstDate)));
            DataAssert.Same<Listing>(q => q.Where(w => Equals(firstDate, w.CreatedAt)));
        }

        [Fact]
        public void NotEqualToConstant()
        {
            var firstDate = DataAssert.Data.Memory<Listing>().First().CreatedAt;

            DataAssert.Same<Listing>(q => q.Where(w => firstDate != w.CreatedAt));
            DataAssert.Same<Listing>(q => q.Where(w => w.CreatedAt != firstDate));
            DataAssert.Same<Listing>(q => q.Where(w => !w.CreatedAt.Equals(firstDate)));
            DataAssert.Same<Listing>(q => q.Where(w => !firstDate.Equals(w.CreatedAt)));
            DataAssert.Same<Listing>(q => q.Where(w => !Equals(w.CreatedAt, firstDate)));
            DataAssert.Same<Listing>(q => q.Where(w => !Equals(firstDate, w.CreatedAt)));
        }
    }
}
