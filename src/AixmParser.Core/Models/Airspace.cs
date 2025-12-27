using NetTopologySuite.Geometries;

namespace AixmParser.Core.Models;

/// <summary>
/// Represents an airspace volume with geometric boundaries and vertical limits.
/// </summary>
public class Airspace
{
    /// <summary>
    /// Gets or sets the unique identifier for this airspace.
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Gets or sets the airspace designator (e.g., "R101", "D12").
    /// </summary>
    public string? Designator { get; set; }

    /// <summary>
    /// Gets or sets the airspace type (e.g., "CTR", "TMA", "FIR", "RESTRICTED").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the airspace name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the lower vertical limit value.
    /// </summary>
    public string? LowerLimit { get; set; }

    /// <summary>
    /// Gets or sets the lower limit reference (e.g., "SFC", "MSL", "AGL").
    /// </summary>
    public string? LowerLimitReference { get; set; }

    /// <summary>
    /// Gets or sets the upper vertical limit value.
    /// </summary>
    public string? UpperLimit { get; set; }

    /// <summary>
    /// Gets or sets the upper limit reference (e.g., "MSL", "FL").
    /// </summary>
    public string? UpperLimitReference { get; set; }

    /// <summary>
    /// Gets or sets the horizontal geometry of the airspace (typically a Polygon or MultiPolygon).
    /// </summary>
    public Geometry? Geometry { get; set; }
}
