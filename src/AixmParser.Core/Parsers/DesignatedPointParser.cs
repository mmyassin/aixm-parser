using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.GeometryParsing;
using AixmParser.Core.Models;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for AIXM DesignatedPoint features.
/// </summary>
internal static class DesignatedPointParser
{
    /// <summary>
    /// Parses all designated points from an AIXM document.
    /// </summary>
    /// <param name="doc">The AIXM XDocument.</param>
    /// <returns>Enumerable of parsed designated points.</returns>
    public static IEnumerable<DesignatedPoint> ParseDesignatedPoints(XDocument doc)
    {
        foreach (var pointElement in doc.Descendants(Namespaces.Aixm + "DesignatedPoint"))
        {
            var point = ParseDesignatedPoint(pointElement);
            if (point != null)
                yield return point;
        }
    }

    /// <summary>
    /// Parses a single designated point element.
    /// </summary>
    private static DesignatedPoint? ParseDesignatedPoint(XElement pointElement)
    {
        var uuid = pointElement.ExtractIdentifier();

        var timeSlice = pointElement
            .Element(Namespaces.Aixm + "timeSlice")?
            .Element(Namespaces.Aixm + "DesignatedPointTimeSlice");

        if (timeSlice == null) return null;

        var designator = timeSlice.GetStringValue(Namespaces.Aixm + "designator");
        var name = timeSlice.GetStringValue(Namespaces.Aixm + "name");
        var type = timeSlice.GetStringValue(Namespaces.Aixm + "type");

        // Parse location
        var location = timeSlice.Element(Namespaces.Aixm + "location");
        var pointGeometry = ParseLocation(location);

        if (pointGeometry == null) return null;

        // Extract latitude and longitude from geometry
        // NTS Point stores coordinates as X=Longitude, Y=Latitude
        double? lon = pointGeometry.X;
        double? lat = pointGeometry.Y;
        double? elevation = pointGeometry.Z;

        return new DesignatedPoint
        {
            Uuid = uuid,
            Designator = designator,
            Name = name,
            Type = type,
            Latitude = lat,
            Longitude = lon,
            Elevation = double.IsNaN(elevation ?? 0) ? null : elevation,
            Geometry = pointGeometry
        };
    }

    /// <summary>
    /// Parses a location element to extract point geometry.
    /// </summary>
    private static NetTopologySuite.Geometries.Point? ParseLocation(XElement? location)
    {
        if (location == null) return null;

        // Try ElevatedPoint first (more detailed)
        var elevatedPoint = location.Element(Namespaces.Aixm + "ElevatedPoint");
        if (elevatedPoint != null)
            return PointParser.ParsePoint(elevatedPoint);

        // Fallback to simple Point
        var simplePoint = location.Element(Namespaces.Aixm + "Point");
        if (simplePoint != null)
            return PointParser.ParsePoint(simplePoint);

        return null;
    }
}
