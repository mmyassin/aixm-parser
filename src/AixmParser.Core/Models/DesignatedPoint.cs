using NetTopologySuite.Geometries;

namespace AixmParser.Core.Models;

/// <summary>
/// Represents a designated point (waypoint) used in air navigation.
/// </summary>
public class DesignatedPoint
{
    /// <summary>
    /// Gets or sets the unique identifier for this designated point.
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Gets or sets the point designator/identifier (e.g., "BABEM", "OKTAL").
    /// </summary>
    public string? Designator { get; set; }

    /// <summary>
    /// Gets or sets the point name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the point type (e.g., "ICAO", "COORD").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the latitude in decimal degrees.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude in decimal degrees.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the elevation of the point.
    /// </summary>
    public double? Elevation { get; set; }

    /// <summary>
    /// Gets or sets the point geometry.
    /// </summary>
    public Point? Geometry { get; set; }
}
