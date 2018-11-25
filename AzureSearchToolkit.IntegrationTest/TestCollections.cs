using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

[assembly: TestCollectionOrderer("AzureSearchToolkit.IntegrationTest.TestExecutionOrderer", "AzureSearchToolkit")]

// Need to turn off test parallelization so we can validate the run order
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    public class TestCollections
    {
        [CollectionDefinition("QueryTestCollection 1")]
        public class QueryTestCollection { }

        [CollectionDefinition("CrudTestCollection 2")]
        public class CrudTestCollection { }
    }
}
