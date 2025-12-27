using NetTopologySuite.Geometries;

namespace AixmParser.Core.Models;

/// <summary>
/// Represents an airport or heliport facility.
/// </summary>
public class AirportHeliport
{
    /// <summary>
    /// Gets or sets the unique identifier for this airport/heliport.
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Gets or sets the location designator.
    /// </summary>
    public string? Designator { get; set; }

    /// <summary>
    /// Gets or sets the airport/heliport name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the ICAO code (e.g., "OMDB", "OMAD").
    /// </summary>
    public string? IcaoCode { get; set; }

    /// <summary>
    /// Gets or sets the facility type (e.g., "AIRPORT", "HELIPORT").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the field elevation value.
    /// </summary>
    public double? FieldElevation { get; set; }

    /// <summary>
    /// Gets or sets the field elevation unit of measurement (e.g., "FT", "M").
    /// </summary>
    public string? FieldElevationUom { get; set; }

    /// <summary>
    /// Gets or sets the vertical datum reference (e.g., "MSL").
    /// </summary>
    public string? VerticalDatum { get; set; }

    /// <summary>
    /// Gets or sets the magnetic variation at the airport in degrees.
    /// </summary>
    public double? MagneticVariation { get; set; }

    /// <summary>
    /// Gets or sets the date of the magnetic variation measurement.
    /// </summary>
    public string? DateMagneticVariation { get; set; }

    /// <summary>
    /// Gets or sets the reference temperature in degrees Celsius.
    /// </summary>
    public double? ReferenceTemperature { get; set; }

    /// <summary>
    /// Gets or sets the transition altitude.
    /// </summary>
    public string? TransitionAltitude { get; set; }

    /// <summary>
    /// Gets or sets the name of the city served by this airport.
    /// </summary>
    public string? ServedCity { get; set; }

    /// <summary>
    /// Gets or sets the ARP (Aerodrome Reference Point) latitude in decimal degrees.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the ARP (Aerodrome Reference Point) longitude in decimal degrees.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the ARP elevation.
    /// </summary>
    public double? ArpElevation { get; set; }

    /// <summary>
    /// Gets or sets the point geometry for the ARP location.
    /// </summary>
    public Point? Geometry { get; set; }
}
