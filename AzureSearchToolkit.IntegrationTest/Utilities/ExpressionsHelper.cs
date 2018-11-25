using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    class ExpressionsHelper
    {
        public  static IOrderedQueryable<TSource> OrderingHelper<TSource>(IQueryable<TSource> source, string propertyName, 
            bool descending = false, bool anotherLevel = false)
        {
            var param = Expression.Parameter(typeof(TSource), string.Empty); 
            var property = Expression.PropertyOrField(param, propertyName);
            var sort = Expression.Lambda(property, param);

            var call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(TSource), property.Type },
                source.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(call);
        }
    }
}
