using System.Xml.Linq;
using AixmParser.Core.Common;
using NetTopologySuite.Geometries;

namespace AixmParser.Core.GeometryParsing;

/// <summary>
/// Provides methods for building NetTopologySuite geometries from GML elements.
/// </summary>
internal static class GeometryBuilder
{
    private static readonly GeometryFactory Factory = new GeometryFactory();

    /// <summary>
    /// Creates a Point geometry from latitude and longitude.
    /// </summary>
    /// <param name="latitude">The latitude in decimal degrees.</param>
    /// <param name="longitude">The longitude in decimal degrees.</param>
    /// <returns>A Point geometry or null if coordinates are invalid.</returns>
    public static Point? CreatePoint(double? latitude, double? longitude)
    {
        if (!latitude.HasValue || !longitude.HasValue)
            return null;

        return Factory.CreatePoint(new Coordinate(longitude.Value, latitude.Value));
    }

    /// <summary>
    /// Parses a GML Curve element into a LineString.
    /// Supports both posList and individual pos elements.
    /// </summary>
    /// <param name="curve">The GML Curve element.</param>
    /// <returns>A LineString or null if parsing fails.</returns>
    public static LineString? ParseCurveToLineString(XElement? curve)
    {
        if (curve == null) return null;

        var geodesicString = curve.Descendants().FirstOrDefault(e => e.Name.LocalName == "GeodesicString");
        if (geodesicString == null) return null;

        // Extract srsName from curve element for proper coordinate ordering
        var srsName = curve.Attribute("srsName")?.Value;

        // Try posList first (single element with all coordinates)
        var posList = geodesicString.Element(Namespaces.Gml + "posList")
                      ?? geodesicString.Elements().FirstOrDefault(e => e.Name.LocalName == "posList");

        Coordinate[] coordinates;

        if (posList != null)
        {
            coordinates = CoordinateParser.ParsePosList(posList, srsName).ToArray();
        }
        else
        {
            // Try multiple pos elements
            var posElements = geodesicString.Elements().Where(e => e.Name.LocalName == "pos").ToList();
            if (!posElements.Any()) return null;

            var coordList = new List<Coordinate>();
            foreach (var posEl in posElements)
            {
                var coord = CoordinateParser.ParsePosCoordinate(posEl, srsName);
                if (coord != null)
                    coordList.Add(coord);
            }
            coordinates = coordList.ToArray();
        }

        if (coordinates.Length < 2) return null;
        return Factory.CreateLineString(coordinates);
    }

    /// <summary>
    /// Parses a GML Surface element into a Polygon.
    /// </summary>
    /// <param name="surface">The GML Surface element.</param>
    /// <returns>A Polygon or null if parsing fails.</returns>
    public static Polygon? ParseSurfaceToPolygon(XElement? surface)
    {
        if (surface == null) return null;

        var patches = surface.Descendants().FirstOrDefault(e => e.Name.LocalName == "patches");
        if (patches == null) return null;

        var polygonPatch = patches.Elements().FirstOrDefault(e => e.Name.LocalName == "PolygonPatch");
        if (polygonPatch == null) return null;

        var exterior = polygonPatch.Element(Namespaces.Gml + "exterior")
                       ?? polygonPatch.Elements().FirstOrDefault(e => e.Name.LocalName == "exterior");
        if (exterior == null) return null;

        var ring = exterior.Element(Namespaces.Gml + "Ring")
                   ?? exterior.Elements().FirstOrDefault(e => e.Name.LocalName == "Ring");
        if (ring == null) return null;

        var curveMember = ring.Element(Namespaces.Gml + "curveMember")
                          ?? ring.Elements().FirstOrDefault(e => e.Name.LocalName == "curveMember");
        if (curveMember == null) return null;

        var curve = curveMember.Element(Namespaces.Gml + "Curve")
                    ?? curveMember.Elements().FirstOrDefault(e => e.Name.LocalName == "Curve");

        var lineString = ParseCurveToLineString(curve);
        if (lineString == null) return null;

        // Ensure the ring is closed
        var coords = lineString.Coordinates;
        if (!coords[0].Equals2D(coords[coords.Length - 1]))
        {
            var closedCoords = new Coordinate[coords.Length + 1];
            Array.Copy(coords, closedCoords, coords.Length);
            closedCoords[coords.Length] = new Coordinate(coords[0].X, coords[0].Y);
            coords = closedCoords;
        }

        var shell = Factory.CreateLinearRing(coords);
        return Factory.CreatePolygon(shell);
    }

    /// <summary>
    /// Combines multiple LineString geometries into a single geometry.
    /// Returns a LineString if only one segment, MultiLineString if multiple.
    /// </summary>
    /// <param name="segments">Collection of route segments with geometries.</param>
    /// <returns>A Geometry (LineString or MultiLineString) or null.</returns>
    public static NetTopologySuite.Geometries.Geometry? CombineLineStrings(IEnumerable<LineString?> segments)
    {
        var validSegments = segments.Where(s => s != null).Cast<LineString>().ToArray();

        if (validSegments.Length == 0)
            return null;

        if (validSegments.Length == 1)
            return validSegments[0];

        return Factory.CreateMultiLineString(validSegments);
    }
}
