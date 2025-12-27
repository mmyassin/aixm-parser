using System.Globalization;
using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.Models;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for AIXM AirportHeliport features.
/// </summary>
internal static class AirportHeliportParser
{
    /// <summary>
    /// Parses all airport/heliport facilities from an AIXM document.
    /// </summary>
    /// <param name="doc">The AIXM XDocument.</param>
    /// <returns>Enumerable of parsed airport/heliport facilities.</returns>
    public static IEnumerable<AirportHeliport> ParseAirportHeliports(XDocument doc)
    {
        foreach (var airportElement in doc.Descendants(Namespaces.Aixm + "AirportHeliport"))
        {
            var airport = ParseAirportHeliport(airportElement);
            if (airport != null)
                yield return airport;
        }
    }

    /// <summary>
    /// Parses a single airport/heliport element.
    /// </summary>
    private static AirportHeliport? ParseAirportHeliport(XElement airportElement)
    {
        var uuid = airportElement.ExtractIdentifier();

        var timeSlice = airportElement
            .Element(Namespaces.Aixm + "timeSlice")?
            .Element(Namespaces.Aixm + "AirportHeliportTimeSlice");

        if (timeSlice == null) return null;

        var designator = timeSlice.GetStringValue(Namespaces.Aixm + "designator");
        var name = timeSlice.GetStringValue(Namespaces.Aixm + "name");
        var icaoCode = timeSlice.GetStringValue(Namespaces.Aixm + "locationIndicatorICAO");
        var type = timeSlice.GetStringValue(Namespaces.Aixm + "type");

        // Parse field elevation
        double? fieldElevation = null;
        string? fieldElevationUom = null;
        var fieldElevEl = timeSlice.Element(Namespaces.Aixm + "fieldElevation");
        if (fieldElevEl != null)
        {
            if (double.TryParse(fieldElevEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var elev))
                fieldElevation = elev;
            fieldElevationUom = (string?)fieldElevEl.Attribute("uom");
        }

        // Parse magnetic variation
        double? magneticVariation = null;
        var magVarEl = timeSlice.Element(Namespaces.Aixm + "magneticVariation");
        if (magVarEl != null && double.TryParse(magVarEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var magVar))
            magneticVariation = magVar;

        var dateMagneticVariation = timeSlice.GetStringValue(Namespaces.Aixm + "dateMagneticVariation");

        // Parse reference temperature
        double? referenceTemperature = null;
        var refTempEl = timeSlice.Element(Namespaces.Aixm + "referenceTemperature");
        if (refTempEl != null && double.TryParse(refTempEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var temp))
            referenceTemperature = temp;

        // Parse transition altitude
        string? transitionAltitude = timeSlice.GetStringValue(Namespaces.Aixm + "transitionAltitude");

        var verticalDatum = timeSlice.GetStringValue(Namespaces.Aixm + "verticalDatum");

        // Parse served city
        string? servedCity = null;
        var cityEl = timeSlice.Element(Namespaces.Aixm + "servedCity")?
                            .Element(Namespaces.Aixm + "City")?
                            .Element(Namespaces.Aixm + "name");
        if (cityEl != null)
            servedCity = cityEl.Value;

        // Parse ARP (Aerodrome Reference Point)
        NetTopologySuite.Geometries.Point? arpGeometry = null;
        double? arpElevation = null;
        var arpEl = timeSlice.Element(Namespaces.Aixm + "ARP");
        if (arpEl != null)
        {
            var elevatedPoint = arpEl.Element(Namespaces.Aixm + "ElevatedPoint");
            if (elevatedPoint != null)
            {
                arpGeometry = PointParser.ParsePoint(elevatedPoint);

                // Extract elevation from ElevatedPoint
                var elevationEl = elevatedPoint.Element(Namespaces.Aixm + "elevation");
                if (elevationEl != null && double.TryParse(elevationEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var elev))
                    arpElevation = elev;
            }
        }

        // Extract latitude and longitude from ARP geometry
        double? lat = arpGeometry?.Y;
        double? lon = arpGeometry?.X;

        return new AirportHeliport
        {
            Uuid = uuid,
            Designator = designator,
            Name = name,
            IcaoCode = icaoCode,
            Type = type,
            FieldElevation = fieldElevation,
            FieldElevationUom = fieldElevationUom,
            VerticalDatum = verticalDatum,
            MagneticVariation = magneticVariation,
            DateMagneticVariation = dateMagneticVariation,
            ReferenceTemperature = referenceTemperature,
            TransitionAltitude = transitionAltitude,
            ServedCity = servedCity,
            Latitude = lat,
            Longitude = lon,
            ArpElevation = arpElevation,
            Geometry = arpGeometry
        };
    }
}
