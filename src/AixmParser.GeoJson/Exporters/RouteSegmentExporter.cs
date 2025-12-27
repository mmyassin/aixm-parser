using AixmParser.Core.Models;
using NetTopologySuite.Features;

namespace AixmParser.GeoJson.Exporters;

/// <summary>
/// Exports route segment data to GeoJSON format.
/// </summary>
internal static class RouteSegmentExporter
{
    /// <summary>
    /// Converts route segments from all routes to a GeoJSON FeatureCollection.
    /// </summary>
    /// <param name="routes">List of routes containing segments to export.</param>
    /// <returns>A FeatureCollection containing route segment features.</returns>
    public static FeatureCollection ToFeatureCollection(IEnumerable<Route> routes)
    {
        var featureCollection = new FeatureCollection();

        foreach (var route in routes)
        {
            foreach (var segment in route.Segments.Where(s => s.Geometry != null))
            {
                var attributes = new AttributesTable
                {
                    { "uuid", segment.Uuid },
                    { "routeDesignator", route.Designator },
                    { "routeUuid", route.Uuid },
                    { "level", segment.Level },
                    { "upperLimit", segment.UpperLimit },
                    { "upperLimitUom", segment.UpperLimitUom },
                    { "upperLimitReference", segment.UpperLimitReference },
                    { "lowerLimit", segment.LowerLimit },
                    { "lowerLimitUom", segment.LowerLimitUom },
                    { "lowerLimitReference", segment.LowerLimitReference },
                    { "pathType", segment.PathType },
                    { "trueTrack", segment.TrueTrack },
                    { "magneticTrack", segment.MagneticTrack },
                    { "reverseTrueTrack", segment.ReverseTrueTrack },
                    { "reverseMagneticTrack", segment.ReverseMagneticTrack },
                    { "length", segment.Length },
                    { "lengthUom", segment.LengthUom },
                    { "navigationType", segment.NavigationType },
                    { "requiredNavigationPerformance", segment.RequiredNavigationPerformance },
                    { "startPointRef", segment.StartPointRef },
                    { "endPointRef", segment.EndPointRef }
                };

                var feature = new Feature(segment.Geometry, attributes);
                featureCollection.Add(feature);
            }
        }

        return featureCollection;
    }
}
