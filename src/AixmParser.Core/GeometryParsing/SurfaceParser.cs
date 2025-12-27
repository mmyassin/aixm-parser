using System.Globalization;
using System.Xml.Linq;
using AixmParser.Core.Common;
using NetTopologySuite.Geometries;

namespace AixmParser.Core.GeometryParsing;

/// <summary>
/// Comprehensive parser for AIXM surface geometries including arcs, circles, and GeoBorder references.
/// </summary>
internal static class SurfaceParser
{
    private static readonly GeometryFactory Factory = new GeometryFactory();

    /// <summary>
    /// Parses a GML Surface element into a Polygon or MultiPolygon, handling all AIXM geometry types.
    /// </summary>
    public static NetTopologySuite.Geometries.Geometry? ParseSurfaceToGeometry(XElement surface, Dictionary<string, List<Coordinate>>? borders = null)
    {
        var srsName = surface.Attribute("srsName")?.Value;
        var patches = surface.Descendants().Where(e => e.Name.LocalName == "PolygonPatch");
        var polygons = new List<Polygon>();

        foreach (var patch in patches)
        {
            var polygon = ParsePolygonPatch(patch, srsName, borders);
            if (polygon != null)
                polygons.Add(polygon);
        }

        if (polygons.Count == 0) return null;
        if (polygons.Count == 1) return polygons[0];
        return Factory.CreateMultiPolygon(polygons.ToArray());
    }

    private static Polygon? ParsePolygonPatch(XElement patch, string? srsName, Dictionary<string, List<Coordinate>>? borders)
    {
        var exterior = patch.Descendants().FirstOrDefault(e => e.Name.LocalName == "exterior");
        if (exterior == null) return null;

        var coordinates = ParseRingCoordinates(exterior, srsName, borders);

        if (coordinates.Count < 4) return null;

        // Ensure ring is closed
        if (!coordinates.First().Equals2D(coordinates.Last()))
            coordinates.Add(new Coordinate(coordinates.First().X, coordinates.First().Y));

        var coordArray = coordinates.ToArray();
        var ring = Factory.CreateLinearRing(coordArray);
        var polygon = Factory.CreatePolygon(ring);

        // Fix invalid geometries
        if (!polygon.IsValid)
            polygon = FixInvalidPolygon(polygon);

        return polygon;
    }

    private static List<Coordinate> ParseRingCoordinates(XElement exterior, string? srsName, Dictionary<string, List<Coordinate>>? borders)
    {
        var coordinates = new List<Coordinate>();
        var curveMembers = exterior.Descendants().Where(e => e.Name.LocalName == "curveMember").ToList();

        for (int i = 0; i < curveMembers.Count; i++)
        {
            var curveMember = curveMembers[i];
            var href = curveMember.Attribute(Namespaces.Xlink + "href")?.Value;

            // Check for GeoBorder reference
            if (!string.IsNullOrEmpty(href) && borders != null)
            {
                var borderUuid = UuidExtensions.NormalizeUuid(href);
                if (borderUuid != null && borders.TryGetValue(borderUuid, out var borderCoords))
                {
                    coordinates.AddRange(borderCoords);
                    continue;
                }
            }

            var curve = curveMember.Descendants().FirstOrDefault(e => e.Name.LocalName == "Curve");
            if (curve == null) continue;

            var prev = coordinates.Count > 0 ? coordinates.Last() : null;
            var next = ExtractFirstCoordinateFromNextCurveMember(curveMembers, i + 1, srsName);

            var curveCoords = ParseCurve(curve, prev, next, srsName);
            coordinates.AddRange(curveCoords);
        }

        return coordinates;
    }

    private static List<Coordinate> ParseCurve(XElement curve, Coordinate? prev, Coordinate? next, string? srsName)
    {
        var allCoordinates = new List<Coordinate>();

        // Check if curve has a segments container (multiple segments in order)
        var segmentsContainer = curve.Descendants().FirstOrDefault(e => e.Name.LocalName == "segments");
        if (segmentsContainer != null)
        {
            var segments = segmentsContainer.Elements().ToList();
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var segmentName = segment.Name.LocalName;

                Coordinate? prevCoord = allCoordinates.Count > 0 ? allCoordinates.Last() : prev;
                Coordinate? nextCoord = (i + 1 < segments.Count) ? ExtractFirstCoordinateFromSegment(segments[i + 1], srsName) : next;

                List<Coordinate>? segmentCoords = null;

                if (segmentName == "GeodesicString")
                {
                    segmentCoords = ParseGeodesicString(segment, srsName);
                }
                else if (segmentName == "ArcByCenterPoint")
                {
                    segmentCoords = ParseArcByCenterPoint(segment, prevCoord, nextCoord, srsName);
                }
                else if (segmentName == "CircleByCenterPoint")
                {
                    segmentCoords = ParseCircleByCenterPoint(segment, srsName);
                }
                else if (segmentName == "Arc")
                {
                    segmentCoords = Parse3PointArc(segment, srsName);
                }

                if (segmentCoords != null && segmentCoords.Any())
                    allCoordinates.AddRange(segmentCoords);
            }

            if (allCoordinates.Any())
                return allCoordinates;
        }

        // Fallback: old single-segment logic for curves without segments container
        // Try GeodesicString
        var geodesic = curve.Descendants().FirstOrDefault(s => s.Name.LocalName == "GeodesicString");
        if (geodesic != null)
        {
            var coords = ParseGeodesicString(geodesic, srsName);
            if (coords != null) return coords;
        }

        // Try ArcByCenterPoint
        var arc = curve.Descendants().FirstOrDefault(s => s.Name.LocalName == "ArcByCenterPoint");
        if (arc != null)
        {
            var arcCoords = ParseArcByCenterPoint(arc, prev, next, srsName);
            if (arcCoords != null) return arcCoords;
        }

        // Try CircleByCenterPoint
        var circle = curve.Descendants().FirstOrDefault(s => s.Name.LocalName == "CircleByCenterPoint");
        if (circle != null)
        {
            var circleCoords = ParseCircleByCenterPoint(circle, srsName);
            if (circleCoords != null) return circleCoords;
        }

        // Try 3-point Arc
        var arc3p = curve.Descendants().FirstOrDefault(s => s.Name.LocalName == "Arc");
        if (arc3p != null)
        {
            var arc3pCoords = Parse3PointArc(arc3p, srsName);
            if (arc3pCoords != null) return arc3pCoords;
        }

        // Fallback: try posList directly
        var posListEl = curve.Descendants().FirstOrDefault(e => e.Name.LocalName == "posList");
        if (posListEl != null)
            return CoordinateParser.ParsePosList(posListEl, srsName).ToList();

        return new List<Coordinate>();
    }

    private static List<Coordinate>? ParseGeodesicString(XElement geodesic, string? srsName)
    {
        // Try posList first
        var posList = GetElement(geodesic, "posList");
        if (posList != null)
            return CoordinateParser.ParsePosList(posList, srsName).ToList();

        // Try multiple pos elements
        var posElements = geodesic.Elements().Where(e => e.Name.LocalName == "pos").ToList();
        if (posElements.Any())
        {
            var coords = new List<Coordinate>();
            foreach (var posEl in posElements)
            {
                var coord = CoordinateParser.ParsePosCoordinate(posEl, srsName);
                if (coord != null)
                    coords.Add(coord);
            }
            return coords;
        }

        return null;
    }

    private static List<Coordinate>? ParseArcByCenterPoint(XElement arc, Coordinate? prev, Coordinate? next, string? srsName)
    {
        var pos = GetElement(arc, "pos");
        var radiusEl = GetElement(arc, "radius");
        var startEl = GetElement(arc, "startAngle");
        var endEl = GetElement(arc, "endAngle");

        if (pos == null || radiusEl == null || startEl == null || endEl == null)
            return null;

        // Get SRS from pos element or parent
        var elementSrs = pos.Attribute("srsName")?.Value ?? srsName;

        var center = CoordinateParser.ParsePosCoordinate(pos, elementSrs);
        if (center == null) return null;

        if (!double.TryParse(startEl.Value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var startAngle) ||
            !double.TryParse(endEl.Value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var endAngle) ||
            !double.TryParse(radiusEl.Value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var radius))
            return null;

        // Transform angles if SRS is Lon,Lat (like CRS84)
        if (IsLonLatOrder(elementSrs))
        {
            // GML ArcByCenterPoint for Lon,Lat SRS: 0 = East, CCW
            // Convert to North-based CW (Aviation Bearing)
            startAngle = 90 - startAngle;
            endAngle = 90 - endAngle;
        }

        // Convert radius to meters
        bool isNauticalMiles = (radiusEl.Attribute("uom")?.Value ?? "").ToLowerInvariant().Contains("nmi");
        var radiusMeters = isNauticalMiles ? radius * 1852.0 : radius;

        return ArcBuilder.CreateArc(center.X, center.Y, radiusMeters, startAngle, endAngle, prev, next);
    }

    private static List<Coordinate>? ParseCircleByCenterPoint(XElement circle, string? srsName)
    {
        var pos = GetElement(circle, "pos");
        var radiusEl = GetElement(circle, "radius");

        if (pos == null || radiusEl == null)
            return null;

        var center = CoordinateParser.ParsePosCoordinate(pos, srsName);
        if (center == null) return null;

        if (!double.TryParse(radiusEl.Value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var radius))
            return null;

        // Convert radius to meters
        bool isNauticalMiles = (radiusEl.Attribute("uom")?.Value ?? "").ToLowerInvariant().Contains("nmi");
        var radiusMeters = isNauticalMiles ? radius * 1852.0 : radius;

        return ArcBuilder.CreateCircle(center.X, center.Y, radiusMeters);
    }

    private static List<Coordinate>? Parse3PointArc(XElement arc, string? srsName)
    {
        var posList = GetElement(arc, "posList");
        if (posList == null) return null;

        var coords = CoordinateParser.ParsePosList(posList, srsName).ToList();
        if (coords.Count < 3) return null;

        return ArcBuilder.CreateArcFrom3Points(coords[0], coords[1], coords[2]);
    }

    private static Coordinate? ExtractFirstCoordinateFromSegment(XElement segment, string? srsName)
    {
        var segmentName = segment.Name.LocalName;

        if (segmentName == "GeodesicString")
        {
            var posList = GetElement(segment, "posList");
            if (posList != null)
            {
                var coords = CoordinateParser.ParsePosList(posList, srsName);
                return coords.FirstOrDefault();
            }

            var posElements = segment.Elements().Where(e => e.Name.LocalName == "pos").ToList();
            if (posElements.Any())
                return CoordinateParser.ParsePosCoordinate(posElements.First(), srsName);
        }
        else if (segmentName == "ArcByCenterPoint" || segmentName == "Arc")
        {
            var pos = GetElement(segment, "pos");
            if (pos != null)
                return CoordinateParser.ParsePosCoordinate(pos, srsName);
        }

        return null;
    }

    private static Coordinate? ExtractFirstCoordinateFromNextCurveMember(List<XElement> curveMembers, int startIndex, string? srsName)
    {
        for (int i = startIndex; i < curveMembers.Count; i++)
        {
            var curve = curveMembers[i].Descendants().FirstOrDefault(e => e.Name.LocalName == "Curve");
            if (curve == null) continue;

            var posListEl = curve.Descendants().FirstOrDefault(e => e.Name.LocalName == "posList");
            if (posListEl != null)
            {
                var coord = CoordinateParser.ParsePosList(posListEl, srsName).FirstOrDefault();
                if (coord != null) return coord;
            }

            var posEl = curve.Descendants().FirstOrDefault(e => e.Name.LocalName == "pos");
            if (posEl != null)
            {
                var coord = CoordinateParser.ParsePosCoordinate(posEl, srsName);
                if (coord != null) return coord;
            }
        }

        return null;
    }

    private static XElement? GetElement(XElement parent, string localName)
    {
        var element = parent.Element(Namespaces.Gml + localName)
                      ?? parent.Element(Namespaces.Aixm + localName);
        if (element != null) return element;

        return parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
    }

    private static Polygon? FixInvalidPolygon(Polygon polygon)
    {
        var _fixed = polygon.Buffer(0);

        if (_fixed is Polygon p)
            return p;

        if (_fixed is MultiPolygon mp)
        {
            var unioned = mp.Union();

            if (unioned is Polygon up)
                return up;

            if (unioned is MultiPolygon ump && ump.NumGeometries > 0)
                return ump.Geometries[0] as Polygon;
        }

        return null;
    }

    /// <summary>
    /// Determines if the SRS name indicates Lon,Lat coordinate ordering.
    /// </summary>
    private static bool IsLonLatOrder(string? srsName)
    {
        if (string.IsNullOrEmpty(srsName)) return false;
        return srsName.Contains("CRS84") || srsName.Contains("OGC:1.3:CRS84");
    }
}
