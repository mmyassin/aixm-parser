using AixmParser.Core.Models;
using NetTopologySuite.Features;

namespace AixmParser.GeoJson.Exporters;

/// <summary>
/// Exports airspace data to GeoJSON format.
/// </summary>
internal static class AirspaceExporter
{
    /// <summary>
    /// Converts a list of airspaces to a GeoJSON FeatureCollection.
    /// </summary>
    /// <param name="airspaces">List of airspaces to export.</param>
    /// <returns>A FeatureCollection containing airspace features.</returns>
    public static FeatureCollection ToFeatureCollection(IEnumerable<Airspace> airspaces)
    {
        var featureCollection = new FeatureCollection();

        foreach (var airspace in airspaces.Where(a => a.Geometry != null))
        {
            var attributes = new AttributesTable
            {
                { "uuid", airspace.Uuid },
                { "designator", airspace.Designator },
                { "type", airspace.Type },
                { "name", airspace.Name },
                { "lowerLimit", airspace.LowerLimit },
                { "lowerLimitReference", airspace.LowerLimitReference },
                { "upperLimit", airspace.UpperLimit },
                { "upperLimitReference", airspace.UpperLimitReference }
            };

            var feature = new Feature(airspace.Geometry, attributes);
            featureCollection.Add(feature);
        }

        return featureCollection;
    }
}
