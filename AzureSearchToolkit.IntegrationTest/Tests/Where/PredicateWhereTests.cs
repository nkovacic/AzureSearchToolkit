using AzureSearchToolkit.IntegrationTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("QueryTestCollection 1")]
    public class PredicateWhereTests
    {
        static readonly IQueryable<Listing> searchListings = DataAssert.Data.SearchQuery<Listing>();
        static readonly IQueryable<Listing> memoryListings = DataAssert.Data.SearchQuery<Listing>();

        [Fact]
        public void CanUseSingleWherePredicate()
        {
            var searchResults = searchListings.Where(SesameSeedPriceUnder400());
            var memoryResults = memoryListings.Where(SesameSeedPriceUnder400());

            Assert.Equal(memoryResults.Count(), searchResults.Count());
        }

        [Fact]
        public void CanChainMultipleWherePredicatesForAnd()
        {
            var searchResults = searchListings.Where(SesameSeedPriceUnder400()).Where(PriceUnder200());
            var memoryResults = memoryListings.Where(SesameSeedPriceUnder400()).Where(PriceUnder200());

            Assert.Equal(memoryResults.Count(), searchResults.Count());
        }

        [Fact]
        public void CanCombineMultipleWherePredicatesWithOrInvoke()
        {
            var searchResults = searchListings.Where(Or.Combine(SesameSeedPriceUnder400(), w => w.Price < 200));
            var memoryResults = memoryListings.Where(Or.Combine(SesameSeedPriceUnder400(), w => w.Price < 200));

            Assert.Equal(memoryResults.Count(), searchResults.Count());
        }

        private static Expression<Func<Listing, bool>> SesameSeedPriceUnder400()
        {
            return w => w.Title == "Sesame Seed" && w.Price < 400;
        }

        private static Expression<Func<Listing, bool>> PriceUnder200()
        {
            return w => w.Price < 200;
        }
    }

    class Or
    {
        public static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T), "p");
            var combined = new ParameterReplacer(parameter).Visit(Expression.OrElse(left.Body, right.Body));

            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        class ParameterReplacer : ExpressionVisitor
        {
            readonly ParameterExpression parameter;

            internal ParameterReplacer(ParameterExpression parameter)
            {
                this.parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return parameter;
            }
        }
    }
}
