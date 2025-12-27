using NetTopologySuite.Geometries;

namespace AixmParser.Core.Models;

/// <summary>
/// Represents a navigation aid facility (VOR, DME, NDB, etc.).
/// </summary>
public class Navaid
{
    /// <summary>
    /// Gets or sets the unique identifier for this navaid.
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Gets or sets the navaid type (e.g., "VOR", "DME", "VOR_DME", "VORTAC", "TACAN", "NDB", "ILS_DME", "LOC", "DVOR").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the navaid designator/identifier (e.g., "DXB", "OMR").
    /// </summary>
    public string? Designator { get; set; }

    /// <summary>
    /// Gets or sets the navaid name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the purpose of the navaid.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Gets or sets the magnetic variation at the navaid location in degrees.
    /// </summary>
    public double? MagneticVariation { get; set; }

    /// <summary>
    /// Gets or sets the date of the magnetic variation measurement.
    /// </summary>
    public string? DateMagneticVariation { get; set; }

    /// <summary>
    /// Gets or sets the channel number (for DME, TACAN).
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Gets or sets the frequency value.
    /// </summary>
    public double? Frequency { get; set; }

    /// <summary>
    /// Gets or sets the frequency unit of measurement (e.g., "MHZ", "KHZ").
    /// </summary>
    public string? FrequencyUom { get; set; }

    /// <summary>
    /// Gets or sets the elevation of the navaid facility.
    /// </summary>
    public double? Elevation { get; set; }

    /// <summary>
    /// Gets or sets the elevation unit of measurement (e.g., "FT", "M").
    /// </summary>
    public string? ElevationUom { get; set; }

    /// <summary>
    /// Gets or sets the vertical datum reference (e.g., "MSL").
    /// </summary>
    public string? VerticalDatum { get; set; }

    /// <summary>
    /// Gets or sets the reference to the airport this navaid serves.
    /// </summary>
    public string? ServedAirportRef { get; set; }

    /// <summary>
    /// Gets or sets the latitude in decimal degrees.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude in decimal degrees.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the point geometry for this navaid location.
    /// </summary>
    public Point? Geometry { get; set; }
}
