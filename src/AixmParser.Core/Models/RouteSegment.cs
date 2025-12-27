using NetTopologySuite.Geometries;

namespace AixmParser.Core.Models;

/// <summary>
/// Represents a segment of an air route connecting two points.
/// </summary>
public class RouteSegment
{
    /// <summary>
    /// Gets or sets the unique identifier for this route segment.
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Gets or sets the reference to the parent route UUID.
    /// </summary>
    public string? RouteRef { get; set; }

    /// <summary>
    /// Gets or sets the flight level or altitude level for this segment.
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// Gets or sets the upper limit value for this segment.
    /// </summary>
    public string? UpperLimit { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement for the upper limit.
    /// </summary>
    public string? UpperLimitUom { get; set; }

    /// <summary>
    /// Gets or sets the upper limit reference (e.g., "STD", "MSL").
    /// </summary>
    public string? UpperLimitReference { get; set; }

    /// <summary>
    /// Gets or sets the lower limit value for this segment.
    /// </summary>
    public string? LowerLimit { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement for the lower limit.
    /// </summary>
    public string? LowerLimitUom { get; set; }

    /// <summary>
    /// Gets or sets the lower limit reference (e.g., "STD", "MSL").
    /// </summary>
    public string? LowerLimitReference { get; set; }

    /// <summary>
    /// Gets or sets the path type (e.g., "GREAT_CIRCLE", "RHUMB_LINE").
    /// </summary>
    public string? PathType { get; set; }

    /// <summary>
    /// Gets or sets the true track in degrees.
    /// </summary>
    public double? TrueTrack { get; set; }

    /// <summary>
    /// Gets or sets the magnetic track in degrees.
    /// </summary>
    public double? MagneticTrack { get; set; }

    /// <summary>
    /// Gets or sets the reverse true track in degrees.
    /// </summary>
    public double? ReverseTrueTrack { get; set; }

    /// <summary>
    /// Gets or sets the reverse magnetic track in degrees.
    /// </summary>
    public double? ReverseMagneticTrack { get; set; }

    /// <summary>
    /// Gets or sets the length of the segment.
    /// </summary>
    public double? Length { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement for the length.
    /// </summary>
    public string? LengthUom { get; set; }

    /// <summary>
    /// Gets or sets the navigation type (e.g., "RNAV").
    /// </summary>
    public string? NavigationType { get; set; }

    /// <summary>
    /// Gets or sets the required navigation performance value.
    /// </summary>
    public string? RequiredNavigationPerformance { get; set; }

    /// <summary>
    /// Gets or sets the reference to the start point (navaid or designated point UUID).
    /// </summary>
    public string? StartPointRef { get; set; }

    /// <summary>
    /// Gets or sets the reference to the end point (navaid or designated point UUID).
    /// </summary>
    public string? EndPointRef { get; set; }

    /// <summary>
    /// Gets or sets the geometry of this segment (typically a LineString).
    /// </summary>
    public NetTopologySuite.Geometries.Geometry? Geometry { get; set; }
}
