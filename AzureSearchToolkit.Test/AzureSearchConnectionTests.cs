using AzureSearchToolkit.Test.TestSupport;
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
        const string Index = "Index";

        [Fact]
        public static void GuardClauses_Constructor()
        {
            Assert.Throws<ArgumentNullException>(() => new AzureSearchConnection(null, null, string.Empty, typeof(Listing)));
            Assert.Throws<ArgumentNullException>(() => new AzureSearchConnection(null, null, "", typeof(Listing)));
            Assert.Throws<ArgumentNullException>(() => new AzureSearchConnection(SearchName, SearchKey, null, typeof(Listing)));
            Assert.Throws<ArgumentNullException>(() => new AzureSearchConnection(SearchName, SearchKey, Index, default(Type)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AzureSearchConnection(SearchName, SearchKey, new Dictionary<Type, string>()));
        }

        [Fact]
        public void GuardClauses_Timeout()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AzureSearchConnection(SearchName, SearchKey, Index, timeout: TimeSpan.FromDays(-1)));
        }
    }
}
