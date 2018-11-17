using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class ProjectionTests
    {
        [Fact]
        public void ProjectWithAnonymousType()
        {
            DataAssert.Same<Listing, object>(q => q.Select(x => new { x.Id }));
        }

        [Fact]
        public void ProjectWithObjectInitializerAndNoContructorArgs()
        {

            DataAssert.Same<Listing>(q => q.Select(x => new Listing { Id = x.Id, Title = x.Title }));
        }

        [Fact]
        public void ProjectWithObjectInitializerAndContructorArgs()
        {
            DataAssert.Same<Listing>(q => q.Select(x => new Listing(x.Id) { Title = x.Title }));
        }

        [Fact]
        public void ProjectWithContructorArgsAndNoObjectInitializer()
        {
            DataAssert.Same<Listing>(q => q.Select(x => new Listing(x.Id)));
        }
    }
}
