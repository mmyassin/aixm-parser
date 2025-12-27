using NetTopologySuite.Geometries;

namespace AixmParser.Core.Models;

/// <summary>
/// Represents an air route with its segments and total path geometry.
/// </summary>
public class Route
{
    /// <summary>
    /// Gets or sets the unique identifier for this route.
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Gets or sets the route designator (e.g., "A1", "B5", "UL123").
    /// </summary>
    public string? Designator { get; set; }

    /// <summary>
    /// Gets or sets the location designator.
    /// </summary>
    public string? LocationDesignator { get; set; }

    /// <summary>
    /// Gets or sets the total length of the route in nautical miles.
    /// </summary>
    public double? TotalLength { get; set; }

    /// <summary>
    /// Gets or sets the list of route segments that make up this route.
    /// </summary>
    public List<RouteSegment> Segments { get; set; } = new();

    /// <summary>
    /// Gets or sets the complete path geometry for the entire route (LineString or MultiLineString).
    /// </summary>
    public Geometry? FullPathGeometry { get; set; }
}
