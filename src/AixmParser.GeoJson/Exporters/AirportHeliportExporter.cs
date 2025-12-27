using AixmParser.Core.Models;
using NetTopologySuite.Features;

namespace AixmParser.GeoJson.Exporters;

/// <summary>
/// Exports airport/heliport data to GeoJSON format.
/// </summary>
internal static class AirportHeliportExporter
{
    /// <summary>
    /// Converts a list of airports/heliports to a GeoJSON FeatureCollection.
    /// </summary>
    /// <param name="airports">List of airports/heliports to export.</param>
    /// <returns>A FeatureCollection containing airport/heliport features.</returns>
    public static FeatureCollection ToFeatureCollection(IEnumerable<AirportHeliport> airports)
    {
        var featureCollection = new FeatureCollection();

        foreach (var airport in airports.Where(a => a.Geometry != null))
        {
            var attributes = new AttributesTable
            {
                { "uuid", airport.Uuid },
                { "designator", airport.Designator },
                { "name", airport.Name },
                { "icaoCode", airport.IcaoCode },
                { "type", airport.Type },
                { "fieldElevation", airport.FieldElevation },
                { "fieldElevationUom", airport.FieldElevationUom },
                { "verticalDatum", airport.VerticalDatum },
                { "magneticVariation", airport.MagneticVariation },
                { "dateMagneticVariation", airport.DateMagneticVariation },
                { "referenceTemperature", airport.ReferenceTemperature },
                { "transitionAltitude", airport.TransitionAltitude },
                { "servedCity", airport.ServedCity },
                { "latitude", airport.Latitude },
                { "longitude", airport.Longitude },
                { "arpElevation", airport.ArpElevation }
            };

            var feature = new Feature(airport.Geometry, attributes);
            featureCollection.Add(feature);
        }

        return featureCollection;
    }
}
