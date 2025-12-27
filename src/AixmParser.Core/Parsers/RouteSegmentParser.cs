using System.Globalization;
using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.GeometryParsing;
using AixmParser.Core.Models;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for AIXM RouteSegment features.
/// </summary>
internal static class RouteSegmentParser
{
    /// <summary>
    /// Parses all route segments from an AIXM document.
    /// </summary>
    /// <param name="doc">The AIXM XDocument.</param>
    /// <returns>Enumerable of parsed route segments.</returns>
    public static IEnumerable<RouteSegment> ParseRouteSegments(XDocument doc)
    {
        // Use LocalName matching for compatibility with inline namespace declarations
        foreach (var segmentElement in doc.Descendants().Where(e => e.Name.LocalName == "RouteSegment"))
        {
            var segment = ParseRouteSegment(segmentElement);
            if (segment != null)
                yield return segment;
        }
    }

    /// <summary>
    /// Parses a single route segment element.
    /// </summary>
    private static RouteSegment? ParseRouteSegment(XElement segmentElement)
    {
        var uuid = segmentElement.ExtractIdentifier();

        // Use LocalName matching for compatibility with inline namespace declarations
        var timeSlice = segmentElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "RouteSegmentTimeSlice");
        if (timeSlice == null) return null;

        var level = (string?)timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "level");

        // Parse upper/lower limits
        double? upperLimit = null;
        string? upperLimitUom = null;
        var upperLimitEl = timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "upperLimit");
        if (upperLimitEl != null)
        {
            if (double.TryParse(upperLimitEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var upper))
                upperLimit = upper;
            upperLimitUom = (string?)upperLimitEl.Attribute("uom");
        }

        var upperLimitReference = (string?)timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "upperLimitReference");

        double? lowerLimit = null;
        string? lowerLimitUom = null;
        var lowerLimitEl = timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "lowerLimit");
        if (lowerLimitEl != null)
        {
            if (double.TryParse(lowerLimitEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var lower))
                lowerLimit = lower;
            lowerLimitUom = (string?)lowerLimitEl.Attribute("uom");
        }

        var lowerLimitReference = (string?)timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "lowerLimitReference");

        // Parse track information
        var pathType = (string?)timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "pathType");

        double? trueTrack = ParseDoubleElement(timeSlice, "trueTrack");
        double? magneticTrack = ParseDoubleElement(timeSlice, "magneticTrack");
        double? reverseTrueTrack = ParseDoubleElement(timeSlice, "reverseTrueTrack");
        double? reverseMagneticTrack = ParseDoubleElement(timeSlice, "reverseMagneticTrack");

        // Parse length
        double? length = null;
        string? lengthUom = null;
        var lengthEl = timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "length");
        if (lengthEl != null)
        {
            if (double.TryParse(lengthEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var len))
                length = len;
            lengthUom = (string?)lengthEl.Attribute("uom");
        }

        // Parse navigation info
        var navigationType = (string?)timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "navigationType");
        var rnpStr = (string?)timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "requiredNavigationPerformance");

        // Parse start point reference
        string? startPointRef = null;
        var startEl = timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "start")?
                               .Descendants().FirstOrDefault(e => e.Name.LocalName == "EnRouteSegmentPoint");
        if (startEl != null)
            startPointRef = ExtractPointReference(startEl);

        // Parse end point reference
        string? endPointRef = null;
        var endEl = timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "end")?
                             .Descendants().FirstOrDefault(e => e.Name.LocalName == "EnRouteSegmentPoint");
        if (endEl != null)
            endPointRef = ExtractPointReference(endEl);

        // Parse route reference
        var routeFormedEl = timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "routeFormed");
        string? routeRef = null;
        if (routeFormedEl != null)
            routeRef = routeFormedEl.ExtractHrefUuid();

        // Parse curve geometry
        NetTopologySuite.Geometries.LineString? geometry = null;
        var curveExtentEl = timeSlice.Elements().FirstOrDefault(e => e.Name.LocalName == "curveExtent");
        var curveEl = curveExtentEl?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Curve");
        if (curveEl != null)
            geometry = GeometryBuilder.ParseCurveToLineString(curveEl);

        return new RouteSegment
        {
            Uuid = uuid,
            RouteRef = routeRef,
            Level = level,
            UpperLimit = upperLimit?.ToString(),
            UpperLimitUom = upperLimitUom,
            UpperLimitReference = upperLimitReference,
            LowerLimit = lowerLimit?.ToString(),
            LowerLimitUom = lowerLimitUom,
            LowerLimitReference = lowerLimitReference,
            PathType = pathType,
            TrueTrack = trueTrack,
            MagneticTrack = magneticTrack,
            ReverseTrueTrack = reverseTrueTrack,
            ReverseMagneticTrack = reverseMagneticTrack,
            Length = length,
            LengthUom = lengthUom,
            NavigationType = navigationType,
            RequiredNavigationPerformance = rnpStr,
            StartPointRef = startPointRef,
            EndPointRef = endPointRef,
            Geometry = geometry
        };
    }

    /// <summary>
    /// Parses a double value from a child element.
    /// </summary>
    private static double? ParseDoubleElement(XElement parent, string elementName)
    {
        var el = parent.Elements().FirstOrDefault(e => e.Name.LocalName == elementName);
        if (el != null && double.TryParse(el.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return value;
        return null;
    }

    /// <summary>
    /// Extracts point reference (fixDesignatedPoint or navaidSystem) from EnRouteSegmentPoint.
    /// </summary>
    private static string? ExtractPointReference(XElement segmentPoint)
    {
        // Try fixDesignatedPoint first
        var fixEl = segmentPoint.Elements().FirstOrDefault(e => e.Name.LocalName == "pointChoice_fixDesignatedPoint");
        if (fixEl != null)
            return fixEl.ExtractHrefUuid();

        // Try navaidSystem
        var navaidEl = segmentPoint.Elements().FirstOrDefault(e => e.Name.LocalName == "pointChoice_navaidSystem");
        if (navaidEl != null)
            return navaidEl.ExtractHrefUuid();

        return null;
    }
}
