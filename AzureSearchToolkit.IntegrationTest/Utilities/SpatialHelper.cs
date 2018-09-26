using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    class SpatialHelper
    {
        

        private static double EarthRadiusInMiles = 3959.0;
        private static double EarthRadiusInNauticalMiles = 3440.0;
        private static double EarthRadiusInKilometers = 6371.0;
        private static double EarthRadiusInMeters = 6371000.0;

        /// <summary>
        /// Gets the radian.
        /// </summary>
        /// <param name="d">The double</param>
        /// <returns></returns>
        public static double ToRadian(double d)
        {
            return d * (Math.PI / 180);
        }

        /// <summary>
        /// Diffs the radian.
        /// </summary>
        /// <param name="val1">First value</param>
        /// <param name="val2">Second value</param>
        /// <returns></returns>
        public static double DiffRadian(double val1, double val2)
        {
            return ToRadian(val2) - ToRadian(val1);
        }

        public static double GetDistance(GeographyPoint originalPoint, GeographyPoint destinationPoint,
            int decimalPlaces = 1, DistanceUnit distanceUnit = DistanceUnit.Kilometers)
        {
            return GetDistance(originalPoint.Latitude, originalPoint.Longitude, destinationPoint.Latitude,
                destinationPoint.Longitude, decimalPlaces, distanceUnit);
        }

        /// <summary>   
        /// Calculate the distance between two sets of coordinates.
        /// <param name="originLatitude">The latitude of the origin location in decimal notation</param>
        /// <param name="originLongitude">The longitude of the origin location in decimal notation</param>
        /// <param name="destinationLatitude">The latitude of the destination location in decimal notation</param>
        /// <param name="destinationLongitude">The longitude of the destination location in decimal notation</param>
        /// <param name="decimalPlaces">The number of decimal places to round the return value to</param>
        /// <param name="distanceUnit">The unit of distance</param>
        /// <returns>A <see cref="Double"/> value representing the distance in from the origin to the destination coordinate</returns>
        /// </summary>
        public static double GetDistance(double originLatitude, double originLongitude, double destinationLatitude,
            double destinationLongitude, int decimalPlaces = 1,  DistanceUnit distanceUnit = DistanceUnit.Meters)
        {
            var radius = GetRadius(distanceUnit);

            return Math.Round(
                    radius * 2 *
                    Math.Asin(Math.Min(1,
                                       Math.Sqrt(
                                           (Math.Pow(Math.Sin(DiffRadian(originLatitude, destinationLatitude) / 2.0), 2.0) +
                                            Math.Cos(ToRadian(originLatitude) * Math.Cos(ToRadian(destinationLatitude))) *
                                            Math.Pow(Math.Sin(DiffRadian(originLongitude, destinationLongitude) / 2.0),
                                                     2.0))))), decimalPlaces);
        }

        private static double GetRadius(DistanceUnit distanceUnit)
        {
            switch (distanceUnit)
            {
                case DistanceUnit.Kilometers:
                    return EarthRadiusInKilometers;
                case DistanceUnit.Meters:
                    return EarthRadiusInMeters;
                case DistanceUnit.NauticalMiles:
                    return EarthRadiusInNauticalMiles;
                default:
                    return EarthRadiusInMiles;
            }
        }
    }
}
