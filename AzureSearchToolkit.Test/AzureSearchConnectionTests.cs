using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.Test
{
    public class AzureSearchConnectionTests
    {
        const string SearchName = "searchName";
        const string SearchKey = "searchKey";

        [Fact]
        public static void GuardClauses_Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureSearchConnection(null, null));
            Assert.Throws<ArgumentException>(() => new AzureSearchConnection(null, null, index: ""));
        }

        [Fact]
        public void GuardClauses_Timeout()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AzureSearchConnection(SearchName, SearchKey, timeout: TimeSpan.FromDays(-1)));
        }
    }
}
