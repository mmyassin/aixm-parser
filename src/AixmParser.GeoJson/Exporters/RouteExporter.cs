using AixmParser.Core.Models;
using NetTopologySuite.Features;

namespace AixmParser.GeoJson.Exporters;

/// <summary>
/// Exports route data to GeoJSON format.
/// </summary>
internal static class RouteExporter
{
    /// <summary>
    /// Converts a list of routes to a GeoJSON FeatureCollection.
    /// </summary>
    /// <param name="routes">List of routes to export.</param>
    /// <returns>A FeatureCollection containing route features.</returns>
    public static FeatureCollection ToFeatureCollection(IEnumerable<Route> routes)
    {
        var featureCollection = new FeatureCollection();

        foreach (var route in routes.Where(r => r.FullPathGeometry != null))
        {
            var attributes = new AttributesTable
            {
                { "uuid", route.Uuid },
                { "designator", route.Designator },
                { "locationDesignator", route.LocationDesignator },
                { "totalLength", route.TotalLength },
                { "segmentCount", route.Segments.Count }
            };

            var feature = new Feature(route.FullPathGeometry, attributes);
            featureCollection.Add(feature);
        }

        return featureCollection;
    }
}
