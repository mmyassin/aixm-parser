using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.GeometryParsing;
using NetTopologySuite.Geometries;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for AIXM GeoBorder features (shared boundary segments).
/// </summary>
internal static class GeoBorderParser
{
    /// <summary>
    /// Parses all GeoBorders from an AIXM document and returns a dictionary of UUID to coordinates.
    /// </summary>
    /// <param name="doc">The AIXM XDocument.</param>
    /// <returns>Dictionary mapping GeoBorder UUIDs to their coordinate lists.</returns>
    public static Dictionary<string, List<Coordinate>> ParseGeoBorders(XDocument doc)
    {
        var borders = new Dictionary<string, List<Coordinate>>(StringComparer.OrdinalIgnoreCase);
        var borderElements = doc.Descendants().Where(e => e.Name.LocalName == "GeoBorder").ToList();

        foreach (var borderElement in borderElements)
        {
            var timeSlice = borderElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "GeoBorderTimeSlice");
            if (timeSlice == null) continue;

            var gmlId = borderElement.Attribute(Namespaces.Gml + "id")?.Value;
            var identifier = borderElement.Element(Namespaces.Gml + "identifier")?.Value;

            var borderGeom = timeSlice.Descendants().FirstOrDefault(e => e.Name.LocalName == "Curve");
            if (borderGeom == null) continue;

            var coords = ParseGeoBorderCurve(borderGeom);
            if (coords != null && coords.Any())
            {
                var normalizedGmlId = UuidExtensions.NormalizeUuid(gmlId);
                var normalizedIdentifier = UuidExtensions.NormalizeUuid(identifier);

                if (normalizedGmlId != null)
                    borders[normalizedGmlId] = coords;
                if (normalizedIdentifier != null)
                    borders[normalizedIdentifier] = coords;
            }
        }

        return borders;
    }

    private static List<Coordinate>? ParseGeoBorderCurve(XElement curve)
    {
        // Get srsName from the curve element or parent
        var srsName = curve.Attribute("srsName")?.Value ?? curve.Parent?.Attribute("srsName")?.Value;

        // GeoBorders typically have simple GeodesicString segments
        var geodesicString = curve.Descendants().FirstOrDefault(e => e.Name.LocalName == "GeodesicString");
        if (geodesicString != null)
        {
            // Try posList first
            var posList = geodesicString.Element(Namespaces.Gml + "posList")
                          ?? geodesicString.Elements().FirstOrDefault(e => e.Name.LocalName == "posList");

            if (posList != null)
            {
                return CoordinateParser.ParsePosList(posList, srsName).ToList();
            }
            else
            {
                // Try multiple pos elements
                var posElements = geodesicString.Elements().Where(e => e.Name.LocalName == "pos").ToList();
                if (posElements.Any())
                {
                    var coordList = new List<Coordinate>();
                    foreach (var posEl in posElements)
                    {
                        var coord = CoordinateParser.ParsePosCoordinate(posEl, srsName);
                        if (coord != null)
                            coordList.Add(coord);
                    }
                    return coordList;
                }
            }
        }

        return null;
    }
}
