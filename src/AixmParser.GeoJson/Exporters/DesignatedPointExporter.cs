using AixmParser.Core.Models;
using NetTopologySuite.Features;

namespace AixmParser.GeoJson.Exporters;

/// <summary>
/// Exports designated point data to GeoJSON format.
/// </summary>
internal static class DesignatedPointExporter
{
    /// <summary>
    /// Converts a list of designated points to a GeoJSON FeatureCollection.
    /// </summary>
    /// <param name="points">List of designated points to export.</param>
    /// <returns>A FeatureCollection containing designated point features.</returns>
    public static FeatureCollection ToFeatureCollection(IEnumerable<DesignatedPoint> points)
    {
        var featureCollection = new FeatureCollection();

        foreach (var point in points.Where(p => p.Geometry != null))
        {
            var attributes = new AttributesTable
            {
                { "uuid", point.Uuid },
                { "designator", point.Designator },
                { "name", point.Name },
                { "type", point.Type },
                { "latitude", point.Latitude },
                { "longitude", point.Longitude },
                { "elevation", point.Elevation }
            };

            var feature = new Feature(point.Geometry, attributes);
            featureCollection.Add(feature);
        }

        return featureCollection;
    }
}
