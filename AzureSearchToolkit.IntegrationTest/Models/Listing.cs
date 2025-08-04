using AzureSearchToolkit.Attributes;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Models
{
    [AzureSearchIndex(Data.Index)]
    [SerializePropertyNamesAsCamelCase]
    class Listing : IEquatable<Listing>
    {
        [Key]
        [IsFilterable]
        public string Id { get; set; }

        [IsFilterable, IsSortable]
        public DateTime? CreatedAt { get; set; }

        [IsSearchable]
        public string Description { get; set; }

        [IsFacetable, IsFilterable, IsSortable]
        public double Price { get; set; }

        [IsFilterable, IsSearchable, IsSortable]
        public string Title { get; set; }

        [IsFilterable]
        public bool Active { get; set; }

        [IsFilterable]
        public string[] Tags { get; set; }

        [IsFilterable, IsSortable, JsonProperty("placeCoordinates")]
        public GeographyPoint Place { get; set; }

        public Listing() { }

        public Listing(string id)
        {
            Id = id;
        }

        public Listing(Listing listing)
        {
            Id = listing.Id;
            CreatedAt = listing.CreatedAt;
            Description = listing.Description;
            Price = listing.Price;
            Title = listing.Title;
            Active = listing.Active;
            Tags = new List<string>(listing.Tags).ToArray();
            Place = listing.Place;
        }

        public bool Equals(Listing other)
        {
            return Equals(Id, other.Id) &&
                   Equals(CreatedAt, other.CreatedAt) &&
                   Equals(Price, other.Price) &&
                   Equals(Title, other.Title) &&
                   Equals(Active, other.Active) &&
                   ((Tags == null && other.Tags == null) ||
                   Enumerable.SequenceEqual(Tags, other.Tags));
        }

        public override bool Equals(object obj)
        {
            return obj is Listing && Equals((Listing)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
