using System.Globalization;
using NetTopologySuite.Geometries;

namespace AixmParser.Core.GeometryParsing;

/// <summary>
/// Provides methods for building arc and circle geometries using geodetic calculations.
/// </summary>
internal static class ArcBuilder
{
    private const double EarthRadiusMeters = 6378137.0;

    /// <summary>
    /// Creates a circle geometry from center point and radius.
    /// </summary>
    /// <param name="centerLon">Center longitude in degrees.</param>
    /// <param name="centerLat">Center latitude in degrees.</param>
    /// <param name="radiusMeters">Radius in meters.</param>
    /// <param name="segments">Number of segments (default 64).</param>
    /// <returns>List of coordinates forming the circle.</returns>
    public static List<Coordinate> CreateCircle(double centerLon, double centerLat, double radiusMeters, int segments = 64)
    {
        var coordinates = new List<Coordinate>(segments + 1);

        for (int i = 0; i <= segments; i++)
        {
            double bearing = 360.0 * i / segments;
            var point = CalculateDestination(centerLat, centerLon, bearing, radiusMeters);
            coordinates.Add(new Coordinate(point.Longitude, point.Latitude));
        }

        return coordinates;
    }

    /// <summary>
    /// Creates an arc geometry from center point, radius, and start/end angles.
    /// </summary>
    /// <param name="centerLon">Center longitude in degrees.</param>
    /// <param name="centerLat">Center latitude in degrees.</param>
    /// <param name="radiusMeters">Radius in meters.</param>
    /// <param name="startAngle">Start angle in degrees (0 = North, clockwise).</param>
    /// <param name="endAngle">End angle in degrees (0 = North, clockwise).</param>
    /// <param name="prev">Previous coordinate for connectivity checking.</param>
    /// <param name="next">Next coordinate for connectivity checking.</param>
    /// <returns>List of coordinates forming the arc.</returns>
    public static List<Coordinate> CreateArc(double centerLon, double centerLat, double radiusMeters,
                                              double startAngle, double endAngle, Coordinate? prev, Coordinate? next)
    {
        // Calculate raw difference to determine direction
        double rawDiff = endAngle - startAngle;

        // Normalize angles to 0-360 range
        double normalizedStart = NormalizeAngle(startAngle);
        double normalizedEnd = NormalizeAngle(endAngle);

        // Respect the AIXM sweep direction and magnitude
        // Negative rawDiff = counter-clockwise, Positive = clockwise
        double span = Math.Abs(rawDiff);
        bool clockwise = rawDiff >= 0;

        // Normalize span to 0-360 range
        if (span > 360) span = span % 360;
        if (span == 0) span = 360;  // Full circle

        // Calculate number of segments based on arc length
        // Use 128 segments for full circle (360°) for smoother arcs
        int segments = Math.Max(16, (int)Math.Round(128 * (span / 360.0)));
        var coordinates = new List<Coordinate>(segments + 1);

        // Generate arc points
        for (int i = 0; i <= segments; i++)
        {
            double bearing = clockwise
                ? normalizedStart + span * i / segments
                : normalizedStart - span * i / segments;

            var point = CalculateDestination(centerLat, centerLon, bearing, radiusMeters);
            coordinates.Add(new Coordinate(point.Longitude, point.Latitude));
        }

        // Check if arc needs to be reversed based on connectivity with neighbors
        double normalScore = CalculateMatchScore(coordinates, prev, next);
        coordinates.Reverse();
        double reversedScore = CalculateMatchScore(coordinates, prev, next);

        // Only reverse if it strictly improves the match (lower distance score)
        if (normalScore <= reversedScore)
            coordinates.Reverse(); // Revert to original if it matches better or EQUALLY well

        return coordinates;
    }

    /// <summary>
    /// Creates an arc from 3 points.
    /// </summary>
    public static List<Coordinate> CreateArcFrom3Points(Coordinate p1, Coordinate p2, Coordinate p3)
    {
        // Find center and radius of the circle passing through 3 points (planar approximation)
        var center = CalculateCircumcenter(p1, p2, p3);
        if (center == null)
            return new List<Coordinate> { p1, p2, p3 }; // Fallback to straight lines if points are collinear

        double radiusMeters = Haversine(center.Y, center.X, p1.Y, p1.X);

        // Calculate bearings from center to points
        double b1 = NormalizeAngle(CalculateBearing(center.Y, center.X, p1.Y, p1.X));
        double b2 = NormalizeAngle(CalculateBearing(center.Y, center.X, p2.Y, p2.X));
        double b3 = NormalizeAngle(CalculateBearing(center.Y, center.X, p3.Y, p3.X));

        // Determine direction (CW or CCW) based on the intermediate point
        double diff12 = NormalizeAngle(b2 - b1);
        double diff13 = NormalizeAngle(b3 - b1);

        bool clockwise = diff12 < diff13;
        double span = clockwise ? diff13 : (360 - diff13);

        // Number of segments
        int segments = Math.Max(16, (int)Math.Round(128 * (span / 360.0)));
        var result = new List<Coordinate>(segments + 1);

        for (int i = 0; i <= segments; i++)
        {
            double bearing = clockwise
                ? b1 + span * i / segments
                : b1 - span * i / segments;

            var point = CalculateDestination(center.Y, center.X, bearing, radiusMeters);
            result.Add(new Coordinate(point.Longitude, point.Latitude));
        }

        return result;
    }

    private static Coordinate? CalculateCircumcenter(Coordinate p1, Coordinate p2, Coordinate p3)
    {
        double x1 = p1.X, y1 = p1.Y;
        double x2 = p2.X, y2 = p2.Y;
        double x3 = p3.X, y3 = p3.Y;

        double D = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
        if (Math.Abs(D) < 1e-10) return null; // Collinear

        double cx = ((x1 * x1 + y1 * y1) * (y2 - y3) + (x2 * x2 + y2 * y2) * (y3 - y1) + (x3 * x3 + y3 * y3) * (y1 - y2)) / D;
        double cy = ((x1 * x1 + y1 * y1) * (x3 - x2) + (x2 * x2 + y2 * y2) * (x1 - x3) + (x3 * x3 + y3 * y3) * (x2 - x1)) / D;

        return new Coordinate(cx, cy);
    }

    private static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        double dLon = ToRadians(lon2 - lon1);
        double rLat1 = ToRadians(lat1);
        double rLat2 = ToRadians(lat2);

        double y = Math.Sin(dLon) * Math.Cos(rLat2);
        double x = Math.Cos(rLat1) * Math.Sin(rLat2) -
                   Math.Sin(rLat1) * Math.Cos(rLat2) * Math.Cos(dLon);

        double bearing = ToDegrees(Math.Atan2(y, x));
        return (bearing + 360) % 360;
    }

    private static double CalculateMatchScore(IReadOnlyList<Coordinate> arc, Coordinate? prev, Coordinate? next)
    {
        double score = 0;

        if (prev != null)
            score += Haversine(prev.Y, prev.X, arc.First().Y, arc.First().X);

        if (next != null)
            score += Haversine(next.Y, next.X, arc.Last().Y, arc.Last().X);

        return score;
    }

    private static double NormalizeAngle(double angle) => ((angle % 360) + 360) % 360;

    private static (double Latitude, double Longitude) CalculateDestination(double lat, double lon, double bearingDeg, double distanceMeters)
    {
        double bearingRad = ToRadians(bearingDeg);
        double latRad = ToRadians(lat);
        double lonRad = ToRadians(lon);
        double angularDistance = distanceMeters / EarthRadiusMeters;

        double newLatRad = Math.Asin(
            Math.Sin(latRad) * Math.Cos(angularDistance) +
            Math.Cos(latRad) * Math.Sin(angularDistance) * Math.Cos(bearingRad));

        double newLonRad = lonRad + Math.Atan2(
            Math.Sin(bearingRad) * Math.Sin(angularDistance) * Math.Cos(latRad),
            Math.Cos(angularDistance) - Math.Sin(latRad) * Math.Sin(newLatRad));

        return (ToDegrees(newLatRad), ToDegrees(newLonRad));
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        double lat1Rad = ToRadians(lat1);
        double lat2Rad = ToRadians(lat2);
        double deltaLat = ToRadians(lat2 - lat1);
        double deltaLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        return EarthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;
}
