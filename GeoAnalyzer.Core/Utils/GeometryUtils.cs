using System;
using GeoAnalyzer.Core.Models.Geometry;

namespace GeoAnalyzer.Core.Utils
{
    public static class GeometryUtils
    {
        public const double Tolerance = 1e-10;

        public static double CalculateDistance(Point p1, Point p2)
        {
            return p1.DistanceTo(p2);
        }

        public static double CalculateArea(Polygon polygon)
        {
            return polygon.GetArea();
        }

        public static double CalculateLength(Polyline polyline)
        {
            return polyline.GetLength();
        }

        public static bool ArePointsEqual(Point p1, Point p2, double tolerance = Tolerance)
        {
            return Math.Abs(p1.X - p2.X) < tolerance && 
                   Math.Abs(p1.Y - p2.Y) < tolerance;
        }
    }
}
