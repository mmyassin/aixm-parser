using System.Globalization;
using System.Xml.Linq;
using AixmParser.Core.Common;
using NetTopologySuite.Geometries;

namespace AixmParser.Core.GeometryParsing;

/// <summary>
/// Provides methods for parsing coordinates from GML elements.
/// </summary>
internal static class CoordinateParser
{
    /// <summary>
    /// Parses a gml:pos element into a Coordinate.
    /// </summary>
    /// <param name="posElement">The pos element containing space-separated coordinate values.</param>
    /// <param name="parentSrsName">Optional parent SRS name to inherit if element doesn't have one.</param>
    /// <returns>A Coordinate or null if parsing fails.</returns>
    public static Coordinate? ParsePosCoordinate(XElement? posElement, string? parentSrsName = null)
    {
        if (posElement == null) return null;

        var coords = posElement.Value.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (coords.Length < 2) return null;

        if (!double.TryParse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var first) ||
            !double.TryParse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var second))
        {
            return null;
        }

        // Check if srsName attribute indicates explicit coordinate ordering
        var srsName = posElement.Attribute("srsName")?.Value ?? parentSrsName;
        bool hasSrsName = !string.IsNullOrEmpty(srsName);

        // Determine coordinate ordering
        bool isLatLonFormat;

        if (hasSrsName)
        {
            // CRS84 is explicitly Lon,Lat. EPSG:4326 is Lat,Lon.
            // Other EPSG codes also generally default to Lat,Lon in GML.
            isLatLonFormat = !IsLonLatOrder(srsName!);
        }
        else
        {
            // Without srsName, detect format heuristically:
            // - If first value > 90, it must be longitude (lat can't exceed 90)
            // - If |first| > |second|, likely LON, LAT (longitudes tend to be larger)
            // - Otherwise assume LAT, LON
            if (Math.Abs(first) > 90)
                isLatLonFormat = false; // First is lon
            else if (Math.Abs(first) > Math.Abs(second))
                isLatLonFormat = false; // Likely LON, LAT
            else
                isLatLonFormat = true; // Assume LAT, LON
        }

        if (isLatLonFormat)
            return new Coordinate(second, first); // Swap LAT,LON to LON,LAT for NTS
        else
            return new Coordinate(first, second); // Already LON,LAT

    }

    /// <summary>
    /// Parses a gml:posList element into a list of Coordinates.
    /// </summary>
    /// <param name="posListElement">The posList element containing space-separated coordinate pairs.</param>
    /// <param name="parentSrsName">Optional parent SRS name to inherit if element doesn't have one.</param>
    /// <returns>A list of Coordinates.</returns>
    public static List<Coordinate> ParsePosList(XElement? posListElement, string? parentSrsName = null)
    {
        var coordinates = new List<Coordinate>();
        if (posListElement == null) return coordinates;

        var values = posListElement.Value.Trim().Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        // Check if srsName attribute indicates explicit coordinate ordering
        var srsName = posListElement.Attribute("srsName")?.Value ?? parentSrsName;
        bool hasSrsName = !string.IsNullOrEmpty(srsName);

        for (int i = 0; i + 1 < values.Length; i += 2)
        {
            if (!double.TryParse(values[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var first) ||
                !double.TryParse(values[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var second))
            {
                continue;
            }

            // Determine coordinate ordering
            bool isLatLonFormat;

            if (hasSrsName)
            {
                isLatLonFormat = !IsLonLatOrder(srsName!);
            }
            else
            {
                // Without srsName, detect format heuristically:
                // - If first value > 90, it must be longitude (lat can't exceed 90)
                // - If |first| > |second|, likely LON, LAT (longitudes tend to be larger)
                // - Otherwise assume LAT, LON
                if (Math.Abs(first) > 90)
                    isLatLonFormat = false; // First is lon
                else if (Math.Abs(first) > Math.Abs(second))
                    isLatLonFormat = false; // Likely LON, LAT
                else
                    isLatLonFormat = true; // Assume LAT, LON
            }

            if (isLatLonFormat)
                coordinates.Add(new Coordinate(second, first)); // Swap LAT,LON to LON,LAT for NTS
            else
                coordinates.Add(new Coordinate(first, second)); // Already LON,LAT
        }

        return coordinates;
    }

    /// <summary>
    /// Parses AIXM position element to extract latitude and longitude.
    /// </summary>
    /// <param name="posElement">The AIXM position element.</param>
    /// <returns>A tuple of (latitude, longitude) or null.</returns>
    public static (double? Latitude, double? Longitude) ParseAixmPosition(XElement? posElement)
    {
        if (posElement == null) return (null, null);

        var latStr = (string?)posElement.Element(Namespaces.Aixm + "lat");
        var lonStr = (string?)posElement.Element(Namespaces.Aixm + "long");

        double? lat = null, lon = null;

        if (double.TryParse(latStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var latValue))
            lat = latValue;

        if (double.TryParse(lonStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var lonValue))
            lon = lonValue;

        return (lat, lon);
    }

    /// <summary>
    /// Determines if the SRS name indicates Lon,Lat coordinate ordering.
    /// </summary>
    /// <param name="srsName">The SRS name from srsName attribute.</param>
    /// <returns>True if coordinates are in Lon,Lat order, false otherwise.</returns>
    private static bool IsLonLatOrder(string srsName)
    {
        if (string.IsNullOrEmpty(srsName)) return false;
        return srsName.Contains("CRS84") || srsName.Contains("OGC:1.3:CRS84");
    }
}
