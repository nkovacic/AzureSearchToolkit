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
            DataAssert.SameSequence(
                DataAssert.Data.AzureSearch<Listing>().OrderBy(w => AzureSearchMethods.Distance(w.Place, filterPoint)).Take(10).ToList(),
                DataAssert.Data.Memory<Listing>()
                    .OrderBy(w => SpatialHelper.GetDistance(w.Place, filterPoint, DistanceUnit.Kilometers)).Take(10).ToList()
            );
        }

        [Fact]
        public void SpatialOrderByDescendingDistance()
        {
            DataAssert.SameSequence(
                DataAssert.Data.AzureSearch<Listing>().OrderByDescending(w => AzureSearchMethods.Distance(w.Place, filterPoint)).Take(10).ToList(),
                DataAssert.Data.Memory<Listing>()
                    .OrderByDescending(w => SpatialHelper.GetDistance(w.Place, filterPoint, DistanceUnit.Kilometers)).Take(10).ToList()
            );
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
