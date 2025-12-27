namespace AixmParser.Core.Models;

/// <summary>
/// Container for all parsed AIXM data including airspaces, routes, navaids, designated points, and airports.
/// </summary>
public class AixmData
{
    /// <summary>
    /// Gets or sets the list of parsed airspaces.
    /// </summary>
    public List<Airspace> Airspaces { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of parsed routes.
    /// </summary>
    public List<Route> Routes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of parsed navigation aids.
    /// </summary>
    public List<Navaid> Navaids { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of parsed designated points.
    /// </summary>
    public List<DesignatedPoint> DesignatedPoints { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of parsed airport/heliport facilities.
    /// </summary>
    public List<AirportHeliport> AirportHeliports { get; set; } = new();
}
