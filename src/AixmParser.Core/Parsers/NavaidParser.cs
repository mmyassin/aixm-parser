using System.Globalization;
using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.Models;

namespace AixmParser.Core.Parsers;

/// <summary>
/// Parser for AIXM Navaid features.
/// </summary>
internal static class NavaidParser
{
    /// <summary>
    /// Parses all navaids from an AIXM document.
    /// </summary>
    /// <param name="doc">The AIXM XDocument.</param>
    /// <returns>Enumerable of parsed navaids.</returns>
    public static IEnumerable<Navaid> ParseNavaids(XDocument doc)
    {
        foreach (var navaidElement in doc.Descendants(Namespaces.Aixm + "Navaid"))
        {
            var navaid = ParseNavaid(navaidElement);
            if (navaid != null)
                yield return navaid;
        }
    }

    /// <summary>
    /// Parses a single navaid element.
    /// </summary>
    private static Navaid? ParseNavaid(XElement navaidElement)
    {
        var uuid = navaidElement.ExtractIdentifier();

        var timeSlice = navaidElement
            .Element(Namespaces.Aixm + "timeSlice")?
            .Element(Namespaces.Aixm + "NavaidTimeSlice");

        if (timeSlice == null) return null;

        var type = timeSlice.GetStringValue(Namespaces.Aixm + "type");
        var designator = timeSlice.GetStringValue(Namespaces.Aixm + "designator");
        var name = timeSlice.GetStringValue(Namespaces.Aixm + "name");
        var purpose = timeSlice.GetStringValue(Namespaces.Aixm + "purpose");

        // Parse magnetic variation
        double? magneticVariation = null;
        var magVarEl = timeSlice.Element(Namespaces.Aixm + "magneticVariation");
        if (magVarEl != null && double.TryParse(magVarEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var magVar))
            magneticVariation = magVar;

        var dateMagneticVariation = timeSlice.GetStringValue(Namespaces.Aixm + "dateMagneticVariation");

        // Parse channel (for DME, TACAN)
        var channel = timeSlice.GetStringValue(Namespaces.Aixm + "channel");

        // Parse frequency (for VOR, NDB)
        double? frequency = null;
        string? frequencyUom = null;
        var frequencyEl = timeSlice.Element(Namespaces.Aixm + "frequency");
        if (frequencyEl != null)
        {
            if (double.TryParse(frequencyEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var freq))
                frequency = freq;
            frequencyUom = (string?)frequencyEl.Attribute("uom");
        }

        // Parse location
        NetTopologySuite.Geometries.Point? pointGeometry = null;
        double? elevation = null;
        string? elevationUom = null;
        string? verticalDatum = null;

        var location = timeSlice.Element(Namespaces.Aixm + "location");
        if (location != null)
        {
            var elevatedPoint = location.Element(Namespaces.Aixm + "ElevatedPoint");
            if (elevatedPoint != null)
            {
                pointGeometry = PointParser.ParsePoint(elevatedPoint);

                // Extract elevation
                var elevationEl = elevatedPoint.Element(Namespaces.Aixm + "elevation");
                if (elevationEl != null)
                {
                    if (double.TryParse(elevationEl.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var elev))
                        elevation = elev;
                    elevationUom = (string?)elevationEl.Attribute("uom");
                }

                verticalDatum = elevatedPoint.GetStringValue(Namespaces.Aixm + "verticalDatum");
            }
        }

        // Parse served airport reference
        string? servedAirportRef = null;
        var servedAirportEl = timeSlice.Element(Namespaces.Aixm + "servedAirport");
        if (servedAirportEl != null)
            servedAirportRef = servedAirportEl.ExtractHrefUuid();

        if (pointGeometry == null) return null;

        // Extract latitude and longitude from geometry
        // NTS Point stores coordinates as X=Longitude, Y=Latitude
        double? lon = pointGeometry.X;
        double? lat = pointGeometry.Y;

        return new Navaid
        {
            Uuid = uuid,
            Type = type,
            Designator = designator,
            Name = name,
            Purpose = purpose,
            MagneticVariation = magneticVariation,
            DateMagneticVariation = dateMagneticVariation,
            Channel = channel,
            Frequency = frequency,
            FrequencyUom = frequencyUom,
            Elevation = elevation,
            ElevationUom = elevationUom,
            VerticalDatum = verticalDatum,
            ServedAirportRef = servedAirportRef,
            Latitude = lat,
            Longitude = lon,
            Geometry = pointGeometry
        };
    }
}
