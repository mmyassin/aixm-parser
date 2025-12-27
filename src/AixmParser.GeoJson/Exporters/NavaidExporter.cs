using AixmParser.Core.Models;
using NetTopologySuite.Features;

namespace AixmParser.GeoJson.Exporters;

/// <summary>
/// Exports navaid data to GeoJSON format.
/// </summary>
internal static class NavaidExporter
{
    /// <summary>
    /// Converts a list of navaids to a GeoJSON FeatureCollection.
    /// </summary>
    /// <param name="navaids">List of navaids to export.</param>
    /// <returns>A FeatureCollection containing navaid features.</returns>
    public static FeatureCollection ToFeatureCollection(IEnumerable<Navaid> navaids)
    {
        var featureCollection = new FeatureCollection();

        foreach (var navaid in navaids.Where(n => n.Geometry != null))
        {
            var attributes = new AttributesTable
            {
                { "uuid", navaid.Uuid },
                { "type", navaid.Type },
                { "designator", navaid.Designator },
                { "name", navaid.Name },
                { "purpose", navaid.Purpose },
                { "magneticVariation", navaid.MagneticVariation },
                { "dateMagneticVariation", navaid.DateMagneticVariation },
                { "channel", navaid.Channel },
                { "frequency", navaid.Frequency },
                { "frequencyUom", navaid.FrequencyUom },
                { "elevation", navaid.Elevation },
                { "elevationUom", navaid.ElevationUom },
                { "verticalDatum", navaid.VerticalDatum },
                { "servedAirportRef", navaid.ServedAirportRef },
                { "latitude", navaid.Latitude },
                { "longitude", navaid.Longitude }
            };

            var feature = new Feature(navaid.Geometry, attributes);
            featureCollection.Add(feature);
        }

        return featureCollection;
    }
}
