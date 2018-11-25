using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace AzureSearchToolkit.IntegrationTest
{
    public class TestExecutionOrderer : ITestCollectionOrderer
    {
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => int.Parse(collection.DisplayName.Last().ToString()));
        }
    }
}
