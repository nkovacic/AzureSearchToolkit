using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    class SpatialHelper
    {
        public static double GetDistance(GeographyPoint originalPoint, GeographyPoint destinationPoint,
            DistanceUnit distanceUnit = DistanceUnit.Kilometers)
        {
            if (originalPoint == null || destinationPoint == null)
            {
                return -1;
            }

            return GetDistance(originalPoint.Latitude, originalPoint.Longitude, destinationPoint.Latitude,
                destinationPoint.Longitude, distanceUnit);
        }

        /// <summary>   
        /// Calculate the distance between two sets of coordinates.
        /// <param name="originLatitude">The latitude of the origin location in decimal notation</param>
        /// <param name="originLongitude">The longitude of the origin location in decimal notation</param>
        /// <param name="destinationLatitude">The latitude of the destination location in decimal notation</param>
        /// <param name="destinationLongitude">The longitude of the destination location in decimal notation</param>
        /// <param name="distanceUnit">The unit of distance</param>
        /// <returns>A <see cref="Double"/> value representing the distance in from the origin to the destination coordinate</returns>
        /// </summary>
        public static double GetDistance(double originalLatitude, double originalLongitude, double destinationLatitude,
            double destinationLongitude, DistanceUnit distanceUnit = DistanceUnit.Kilometers)
        {
            var rlat1 = Math.PI * originalLatitude / 180;
            var rlat2 = Math.PI * destinationLatitude / 180;
            var theta = originalLongitude - destinationLongitude;
            var rtheta = Math.PI * theta / 180;
            var dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (distanceUnit)
            {
                case DistanceUnit.Kilometers: //Kilometers -> default
                    return dist * 1.609344;
                case DistanceUnit.NauticalMiles: //Nautical Miles 
                    return dist * 0.8684;
                case DistanceUnit.Miles: //Miles
                    return dist;
            }

            return dist;
        }
    }
}