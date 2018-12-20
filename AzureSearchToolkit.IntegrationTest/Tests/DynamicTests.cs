using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    public class DynamicTests
    {
        const string Title = "Sesame Seed";

        [Fact]
        public void DynamicEqualToConstant()
        {
            /*
            Equals to var azureSearchContext = new AzureSearchContext(....);
            var azureSearchIQueriable = azureSearchContext.Query<Listing>();
            */
            var azureSearchIQueriable = DataAssert.Data.SearchQuery<Listing>();

            var source = Expression.Parameter(typeof(Listing), "listing");
            var sourceProperty = Expression.Property(source, "Title");
            var comparisonCall =  Expression.Equal(sourceProperty, Expression.Constant(Title));

            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { azureSearchIQueriable.ElementType },
                azureSearchIQueriable.Expression,
                Expression.Lambda<Func<Listing, bool>>(comparisonCall, new ParameterExpression[] { source }));

            azureSearchIQueriable = azureSearchIQueriable.Provider.CreateQuery<Listing>(whereCallExpression);

            DataAssert.Same(azureSearchIQueriable,
                DataAssert.Data.Memory<Listing>().Where(w => Title == w.Title));
        }
    }
}
