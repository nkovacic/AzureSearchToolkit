using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class StringWhereTests
    {
        const string Title = "Sesame Seed";

        [Fact]
        public void EqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => Title == w.Title));
            DataAssert.Same<Listing>(q => q.Where(w => w.Title == Title));
            DataAssert.Same<Listing>(q => q.Where(w => w.Title.Equals(Title)));
            DataAssert.Same<Listing>(q => q.Where(w => Title.Equals(w.Title)));
            DataAssert.Same<Listing>(q => q.Where(w => Equals(w.Title, Title)));
            DataAssert.Same<Listing>(q => q.Where(w => Equals(Title, w.Title)));
        }

        [Fact]
        public void NotEqualToConstant()
        {
            DataAssert.Same<Listing>(q => q.Where(w => Title != w.Title));
            DataAssert.Same<Listing>(q => q.Where(w => w.Title != Title));
            DataAssert.Same<Listing>(q => q.Where(w => !w.Title.Equals(Title)));
            DataAssert.Same<Listing>(q => q.Where(w => !Title.Equals(w.Title)));
            DataAssert.Same<Listing>(q => q.Where(w => !Equals(w.Title, Title)));
            DataAssert.Same<Listing>(q => q.Where(w => !Equals(Title, w.Title)));
        }
    }
}
