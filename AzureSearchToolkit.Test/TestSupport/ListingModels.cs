using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Test.TestSupport
{
    public class Listing
    {
        public string Id { get; set; }

        public decimal Price { get; set; }
        public string Title { get; set; }

        public bool Active { get; set; }

        public string[] Tags { get; set; }
    }

    public static class ListingFactory
    {
        public static List<Listing> Listings = new List<Listing>
        {
            new Listing
            {
                Id = Guid.NewGuid().ToString(), Active = true, Price = 1500, Tags = new [] { "computer" }, Title = "Medium desktop"
            },
            new Listing
            {
                Id = Guid.NewGuid().ToString(), Active = false, Price = 100, Tags = new [] { "phone" }, Title = "Cheap mobile phone"
            },
            new Listing
            {
                Id = Guid.NewGuid().ToString(), Active = true, Price = 1000, Tags = new [] { "phone" }, Title = "Expensive mobile phone"
            },
            new Listing
            {
                Id = Guid.NewGuid().ToString(), Active = true, Price = 500, Tags = new [] { "phone" }, Title = "Medium mobile phone"
            }
        };
    }
}
