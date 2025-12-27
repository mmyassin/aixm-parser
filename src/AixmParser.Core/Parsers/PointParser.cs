using System.Globalization;
using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.GeometryParsing;
using NetTopologySuite.Geometries;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for GML Point and ElevatedPoint elements.
/// </summary>
internal static class PointParser
{
    private static readonly GeometryFactory Factory = new GeometryFactory();

    /// <summary>
    /// Parses a Point or ElevatedPoint element into a NetTopologySuite Point.
    /// </summary>
    /// <param name="pointElement">The point element to parse.</param>
    /// <returns>A Point geometry or null if parsing fails.</returns>
    public static Point? ParsePoint(XElement? pointElement)
    {
        if (pointElement == null) return null;

        // Try to get the pos element
        var pos = pointElement.Element(Namespaces.Gml + "pos")
                  ?? pointElement.Elements().FirstOrDefault(e => e.Name.LocalName == "pos");

        if (pos == null) return null;

        // Parse coordinate with proper ordering detection
        var coord = CoordinateParser.ParsePosCoordinate(pos);
        if (coord == null) return null;

        // Check if there's elevation (Z coordinate)
        var coordinates = pos.Value.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        double? elevation = null;
        if (coordinates.Length >= 3 && double.TryParse(coordinates[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var elev))
        {
            elevation = elev;
        }
        else
        {
            // Try to get elevation from separate element (for ElevatedPoint)
            var elevationEl = pointElement.Element(Namespaces.Aixm + "elevation")
                              ?? pointElement.Elements().FirstOrDefault(e => e.Name.LocalName == "elevation");

            if (elevationEl != null && double.TryParse(elevationEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var elevValue))
            {
                elevation = elevValue;
            }
        }

        // Create point with or without Z coordinate (coord.X is lon, coord.Y is lat)
        if (elevation.HasValue)
            return Factory.CreatePoint(new CoordinateZ(coord.X, coord.Y, elevation.Value));
        else
            return Factory.CreatePoint(coord);
    }
}
