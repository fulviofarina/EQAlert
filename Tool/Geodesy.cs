using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQAlert
{
    public sealed class Geodesy
    {
        public static double RadToDeg(double radians)
        {
            return radians * (180 / Math.PI);
        }

        public static double DegToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        //Calculate distance between two coordinates useing the Haversine formula  
        public static double DistanceBetweenCoords(double lat1, double long1, double lat2, double long2)
        {
            //Convert input values to radians  
            lat1 = Geodesy.DegToRad(lat1);
            long1 = Geodesy.DegToRad(long1);
            lat2 = Geodesy.DegToRad(lat2);
            long2 = Geodesy.DegToRad(long2);

            double earthRadius = 6371; // km  
            double deltaLat = lat2 - lat1;
            double deltaLong = long2 - long1;
            double a = Math.Sin(deltaLat / 2.0) * Math.Sin(deltaLat / 2.0) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(deltaLong / 2.0) * Math.Sin(deltaLong / 2.0);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = earthRadius * c;

            return distance;
        }
    }

}
