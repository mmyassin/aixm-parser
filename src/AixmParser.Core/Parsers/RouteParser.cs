using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.Models;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for AIXM Route features.
/// </summary>
internal static class RouteParser
{
    /// <summary>
    /// Parses all routes from an AIXM document.
    /// </summary>
    /// <param name="doc">The AIXM XDocument.</param>
    /// <returns>Enumerable of parsed routes.</returns>
    public static IEnumerable<Route> ParseRoutes(XDocument doc)
    {
        foreach (var routeElement in doc.Descendants(Namespaces.Aixm + "Route"))
        {
            var route = ParseRoute(routeElement);
            if (route != null)
                yield return route;
        }
    }

    /// <summary>
    /// Parses a single route element.
    /// </summary>
    private static Route? ParseRoute(XElement routeElement)
    {
        var uuid = routeElement.ExtractIdentifier();

        var timeSlice = routeElement
            .Element(Namespaces.Aixm + "timeSlice")?
            .Element(Namespaces.Aixm + "RouteTimeSlice");

        if (timeSlice == null) return null;

        var designatorPrefix = timeSlice.GetStringValue(Namespaces.Aixm + "designatorPrefix");
        var designatorSecondLetter = timeSlice.GetStringValue(Namespaces.Aixm + "designatorSecondLetter");

        int? designatorNumber = null;
        var designatorNumberEl = timeSlice.Element(Namespaces.Aixm + "designatorNumber");
        if (designatorNumberEl != null && int.TryParse(designatorNumberEl.Value, out var num))
            designatorNumber = num;

        var multipleIdentifier = timeSlice.GetStringValue(Namespaces.Aixm + "multipleIdentifier");
        var locationDesignator = timeSlice.GetStringValue(Namespaces.Aixm + "locationDesignator");

        // Build full designator
        string? designator = BuildRouteDesignator(designatorPrefix, designatorSecondLetter, designatorNumber, multipleIdentifier);

        return new Route
        {
            Uuid = uuid,
            Designator = designator,
            LocationDesignator = locationDesignator
        };
    }

    /// <summary>
    /// Builds a route designator from its components.
    /// </summary>
    private static string? BuildRouteDesignator(string? prefix, string? secondLetter, int? number, string? multipleId)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(prefix))
            parts.Add(prefix);

        if (!string.IsNullOrEmpty(secondLetter))
            parts.Add(secondLetter);

        if (number.HasValue)
            parts.Add(number.Value.ToString());

        if (!string.IsNullOrEmpty(multipleId))
            parts.Add(multipleId);

        return parts.Count > 0 ? string.Join("", parts) : null;
    }
}
