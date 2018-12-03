using AzureSearchToolkit.Async;
using AzureSearchToolkit.IntegrationTest.Models;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    [Collection("CrudTestCollection 2")]
    public class CrudTests
    {
        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void AddDocument()
        {
            var newListingTemplate = await DataAssert.Data.SearchQuery<Listing>().FirstOrDefaultAsync();
            var allListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

            newListingTemplate.Id = Guid.NewGuid().ToString();

            var wasCreated = await DataAssert.Data.SearchContext().AddAsync(newListingTemplate);

            Assert.True(wasCreated);

            var createdListing = await GetListingAfterChange(q => q.Id == newListingTemplate.Id);
            var newAllListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

            await DataAssert.Data.SearchContext().RemoveAsync(newListingTemplate);

            DataAssert.Data.WaitForSearchOperationCompletion(allListingsCount);

            Assert.Equal(newListingTemplate, createdListing);
            Assert.Equal(allListingsCount + 1, newAllListingsCount);
        }

        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void AddWithAddOrUpdateDocument()
        {
            var newListingTemplate = await DataAssert.Data.SearchQuery<Listing>().FirstOrDefaultAsync();
            var allListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

            newListingTemplate.Id = Guid.NewGuid().ToString();

            var wasCreated = await DataAssert.Data.SearchContext().AddOrUpdateAsync(newListingTemplate);

            Assert.True(wasCreated);

            var createdListing = await GetListingAfterChange(q => q.Id == newListingTemplate.Id);
            var newAllListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

            await DataAssert.Data.SearchContext().RemoveAsync(newListingTemplate);

            DataAssert.Data.WaitForSearchOperationCompletion(allListingsCount);

            Assert.Equal(newListingTemplate, createdListing);
            Assert.Equal(allListingsCount + 1, newAllListingsCount);
        }

        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void AddWithChangeDocument()
        {
            await CrudTest(async (listing) =>
            {
                listing.Id = Guid.NewGuid().ToString();

                var wasCreated = await DataAssert.Data.SearchContext().ChangeDocumentAsync(listing, IndexActionType.Upload);

                Assert.True(wasCreated);

                return listing;
            }, IndexActionType.Upload);
        }

        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void RemoveDocument()
        {
            var firstListing = await DataAssert.Data.SearchQuery<Listing>().FirstOrDefaultAsync();
            var allListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

            var wasRemoved = await DataAssert.Data.SearchContext().RemoveAsync(firstListing);

            Assert.True(wasRemoved);

            var removedListing = await GetListingAfterChange(q => q.Id == firstListing.Id, true);
            var newAllListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

            await DataAssert.Data.SearchContext().AddAsync(firstListing);

            DataAssert.Data.WaitForSearchOperationCompletion(allListingsCount);

            Assert.Null(removedListing);
            Assert.Equal(allListingsCount - 1, newAllListingsCount);
        }

        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void RemoveWithChangeDocument()
        {
            await CrudTest(async (listing) =>
            {
                var wasDeleted = await DataAssert.Data.SearchContext().ChangeDocumentAsync(listing, IndexActionType.Delete);

                Assert.True(wasDeleted);

                return listing;
            }, IndexActionType.Delete);
        }

        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void UpdateDocument()
        {
            var listing = await DataAssert.Data.SearchQuery<Listing>().FirstOrDefaultAsync();
            var originalListing = new Listing(listing);

            listing.Title += "a";

            var wasUpdated = await DataAssert.Data.SearchContext().UpdateAsync(listing);

            Assert.True(wasUpdated);

            var updatedListing = await GetListingAfterChange(q => q.Id == listing.Id);

            await DataAssert.Data.SearchContext().UpdateAsync(originalListing);

            Assert.Equal(listing, updatedListing);
        }

        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void UpdateWithAddOrUpdateDocument()
        {
            var listing = await DataAssert.Data.SearchQuery<Listing>().FirstOrDefaultAsync();
            var originalListing = new Listing(listing);

            listing.Title += "a";

            var wasUpdated = await DataAssert.Data.SearchContext().AddOrUpdateAsync(listing);

            Assert.True(wasUpdated);

            var updatedListing = await GetListingAfterChange(q => q.Id == listing.Id);

            await DataAssert.Data.SearchContext().UpdateAsync(originalListing);

            Assert.Equal(listing, updatedListing);
        }

        [Trait("TestCollection", "Crud")]
        [Fact]
        public async void UpdateWithChangeDocument()
        {
            await CrudTest(async (listing) =>
            {
                listing.Title += "a";

                var wasUpdated = await DataAssert.Data.SearchContext().ChangeDocumentAsync(listing, IndexActionType.Merge);

                Assert.True(wasUpdated);

                return listing;
            }, IndexActionType.Merge);
        }

        private async Task CrudTest(Func<Listing, Task<Listing>> crudAction, IndexActionType indexActionType)
        {
            var changingCrudActions = new[] { IndexActionType.Delete, IndexActionType.Upload };

            var listing = await DataAssert.Data.SearchQuery<Listing>().FirstOrDefaultAsync();
            var originalListing = new Listing(listing);
            var allListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

            Listing crudActionResult = await crudAction(listing);

            if (changingCrudActions.Contains(indexActionType))
            {
                var isDeleteAction = indexActionType == IndexActionType.Delete;
                var allListingsShouldCount = allListingsCount + (isDeleteAction ? - 1 : + 1);
                var changedListing = await GetListingAfterChange(q => q.Id == listing.Id, isDeleteAction);
                var newAllListingsCount = await DataAssert.Data.SearchQuery<Listing>().CountAsync();

                if (isDeleteAction)
                {
                    Assert.Null(changedListing);

                    await DataAssert.Data.SearchContext().AddAsync(originalListing);
                }
                else
                {
                    Assert.Equal(crudActionResult, changedListing);

                    await DataAssert.Data.SearchContext().RemoveAsync(originalListing);
                }

                DataAssert.Data.WaitForSearchOperationCompletion(allListingsCount);

                Assert.Equal(allListingsShouldCount, newAllListingsCount);
                
            }
            else
            {
                var updatedListing = await GetListingAfterChange(q => q.Id == listing.Id);

                await DataAssert.Data.SearchContext().UpdateAsync(originalListing);

                Assert.Equal(listing, updatedListing);
            }
        }

        private async Task<Listing> GetListingAfterChange(Expression<Func<Listing, bool>> query, bool shouldReturnNull = false)
        {
            Listing foundListing = null;

            var maxRetryCount = 30;
            var retryCount = 0;

            do
            {
                await Task.Delay(2000);

                foundListing = await DataAssert.Data.SearchQuery<Listing>().FirstOrDefaultAsync(query);
                retryCount++;
            }
            while (((shouldReturnNull && foundListing != null) || (!shouldReturnNull && foundListing == null)) && retryCount < maxRetryCount);

            return foundListing;
        }
    }
}
