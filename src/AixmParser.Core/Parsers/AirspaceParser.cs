using System.Globalization;
using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.GeometryParsing;
using AixmParser.Core.Models;
using NetTopologySuite.Geometries;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for AIXM Airspace features.
/// </summary>
internal static class AirspaceParser
{
    /// <summary>
    /// Parses all airspaces from an AIXM document.
    /// </summary>
    /// <param name="doc">The AIXM XDocument.</param>
    /// <param name="borders">Optional GeoBorder cache for resolving xlink:href references.</param>
    /// <returns>Enumerable of parsed airspaces.</returns>
    public static IEnumerable<Airspace> ParseAirspaces(XDocument doc, Dictionary<string, List<Coordinate>>? borders = null)
    {
        foreach (var airspaceElement in doc.Descendants(Namespaces.Aixm + "Airspace"))
        {
            var airspace = ParseAirspace(airspaceElement, borders);
            if (airspace != null)
                yield return airspace;
        }
    }

    /// <summary>
    /// Parses a single airspace element.
    /// </summary>
    private static Airspace? ParseAirspace(XElement airspaceElement, Dictionary<string, List<Coordinate>>? borders)
    {
        var uuid = airspaceElement.ExtractIdentifier();

        var timeSlice = airspaceElement
            .Element(Namespaces.Aixm + "timeSlice")?
            .Element(Namespaces.Aixm + "AirspaceTimeSlice");

        if (timeSlice == null) return null;

        var designator = timeSlice.GetStringValue(Namespaces.Aixm + "designator");
        var type = timeSlice.GetStringValue(Namespaces.Aixm + "type");
        var name = timeSlice.GetStringValue(Namespaces.Aixm + "name");

        var geometry = ParseAirspaceGeometry(timeSlice, borders);

        var (lowerLimit, lowerRef, upperLimit, upperRef) = ParseVerticalLimits(timeSlice);

        return new Airspace
        {
            Uuid = uuid,
            Designator = designator,
            Type = type,
            Name = name,
            LowerLimit = lowerLimit,
            LowerLimitReference = lowerRef,
            UpperLimit = upperLimit,
            UpperLimitReference = upperRef,
            Geometry = geometry
        };
    }

    /// <summary>
    /// Parses airspace geometry from geometry components using comprehensive surface parser.
    /// </summary>
    private static NetTopologySuite.Geometries.Geometry? ParseAirspaceGeometry(XElement timeSlice, Dictionary<string, List<Coordinate>>? borders)
    {
        var geometries = new List<NetTopologySuite.Geometries.Geometry>();

        var geometryComponents = timeSlice
            .Elements(Namespaces.Aixm + "geometryComponent")
            .Select(gc => gc.Element(Namespaces.Aixm + "AirspaceGeometryComponent"))
            .Where(x => x != null);

        foreach (var component in geometryComponents)
        {
            var surface = component!.Descendants(Namespaces.Aixm + "Surface").FirstOrDefault()
                          ?? component.Descendants(Namespaces.Gml + "Surface").FirstOrDefault();

            if (surface == null) continue;

            var geom = SurfaceParser.ParseSurfaceToGeometry(surface, borders);
            if (geom != null) geometries.Add(geom);
        }

        if (geometries.Count == 0) return null;
        if (geometries.Count == 1) return geometries[0];

        // If multiple geometries, union them
        var factory = new NetTopologySuite.Geometries.GeometryFactory();
        var collection = factory.CreateGeometryCollection(geometries.ToArray());
        return collection.Union();
    }

    /// <summary>
    /// Parses vertical limits (lower and upper bounds) from an airspace time slice.
    /// </summary>
    private static (string? lowerLimit, string? lowerRef, string? upperLimit, string? upperRef) ParseVerticalLimits(XElement timeSlice)
    {
        var volume = timeSlice.Descendants(Namespaces.Aixm + "AirspaceVolume").FirstOrDefault();
        if (volume == null) return (null, null, null, null);

        double? lowerLimitVal = null, upperLimitVal = null;
        string? lowerRef = null, upperRef = null;

        var lowerLimitEl = volume.Element(Namespaces.Aixm + "lowerLimit");
        if (lowerLimitEl != null && double.TryParse(lowerLimitEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var lval))
            lowerLimitVal = lval;

        var upperLimitEl = volume.Element(Namespaces.Aixm + "upperLimit");
        if (upperLimitEl != null && double.TryParse(upperLimitEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var uval))
            upperLimitVal = uval;

        lowerRef = volume.GetStringValue(Namespaces.Aixm + "lowerLimitReference");
        upperRef = volume.GetStringValue(Namespaces.Aixm + "upperLimitReference");

        return (lowerLimitVal?.ToString(), lowerRef, upperLimitVal?.ToString(), upperRef);
    }
}
