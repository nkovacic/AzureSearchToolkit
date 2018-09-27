using AzureSearchToolkit.IntegrationTest.Models;
using AzureSearchToolkit.IntegrationTest.Utilities;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AzureSearchToolkit.IntegrationTest.Tests
{
    public class SpatialTests
    {
        static readonly GeographyPoint filterPoint = GeographyPoint.Create(-21.18442142, -128.12241032);


        [Fact]
        public void SpatialOrderByDistance()
        {
            DataAssert.Same<Listing>(q => q.OrderBy(w => AzureSearchMethods.Distance(w.Place, filterPoint)));
        }

        [Fact]
        public void SpatialOrderByDescendingDistance()
        {
            DataAssert.Same<Listing>(q => q.OrderByDescending(w => AzureSearchMethods.Distance(w.Place, filterPoint)));
        }

        [Fact]
        public void SpatialFilterByDistance()
        {
            DataAssert.SameSequence(
                DataAssert.Data.AzureSearch<Listing>().Where(w => AzureSearchMethods.Distance(w.Place, filterPoint) < 10000).ToList(),
                DataAssert.Data.Memory<Listing>()
                    .Where(w => SpatialHelper.GetDistance(w.Place, filterPoint, DistanceUnit.Kilometers) < 10000).ToList()
            );
        }
    }
}
