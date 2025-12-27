using System.Xml.Linq;
using AixmParser.Core.Common;
using AixmParser.Core.GeometryParsing;
using AixmParser.Core.Models;
using AixmParser.Core.Parsers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AixmParser.Core;

/// <summary>
/// Main entry point for parsing AIXM 5.1/5.1.1 XML documents.
/// Provides access to parsed aeronautical data including airspaces, routes, navaids, and more.
/// </summary>
public class AixmDocument
{
    private readonly XDocument _document;
    private readonly ILogger _logger;
    private AixmData? _cachedData;

    /// <summary>
    /// Gets the parsed AIXM data containing all features.
    /// Data is lazily loaded and cached on first access.
    /// </summary>
    public AixmData Data
    {
        get
        {
            if (_cachedData == null)
            {
                _cachedData = ParseAll();
            }
            return _cachedData;
        }
    }

    private AixmDocument(XDocument document, ILogger? logger = null)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Loads an AIXM document from a file path.
    /// </summary>
    /// <param name="filePath">Path to the AIXM XML file.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <returns>An AixmDocument instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static AixmDocument Load(string filePath, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"AIXM file not found: {filePath}", filePath);

        logger?.LogInformation("Loading AIXM document from: {FilePath}", filePath);
        var doc = XDocument.Load(filePath);
        return new AixmDocument(doc, logger);
    }

    /// <summary>
    /// Loads an AIXM document from a stream.
    /// </summary>
    /// <param name="stream">Stream containing the AIXM XML data.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <returns>An AixmDocument instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    public static AixmDocument Load(Stream stream, ILogger? logger = null)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        logger?.LogInformation("Loading AIXM document from stream");
        var doc = XDocument.Load(stream);
        return new AixmDocument(doc, logger);
    }

    /// <summary>
    /// Parses an AIXM document from an XDocument.
    /// </summary>
    /// <param name="document">The XDocument containing AIXM data.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <returns>An AixmDocument instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when document is null.</exception>
    public static AixmDocument Parse(XDocument document, ILogger? logger = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        return new AixmDocument(document, logger);
    }

    /// <summary>
    /// Parses all AIXM features from the document and links relationships.
    /// </summary>
    /// <returns>AixmData containing all parsed features.</returns>
    private AixmData ParseAll()
    {
        _logger.LogInformation("Starting AIXM data parsing");

        // Parse GeoBorders first (needed for airspace geometry resolution)
        _logger.LogDebug("Parsing GeoBorders");
        var borders = GeoBorderParser.ParseGeoBorders(_document);
        _logger.LogInformation("Parsed {Count} GeoBorders", borders.Count);

        var airspaces = AirspaceParser.ParseAirspaces(_document, borders).ToList();
        _logger.LogInformation("Parsed {Count} airspaces", airspaces.Count);

        var designatedPoints = DesignatedPointParser.ParseDesignatedPoints(_document).ToList();
        _logger.LogInformation("Parsed {Count} designated points", designatedPoints.Count);

        var airports = AirportHeliportParser.ParseAirportHeliports(_document).ToList();
        _logger.LogInformation("Parsed {Count} airports/heliports", airports.Count);

        var navaids = NavaidParser.ParseNavaids(_document).ToList();
        _logger.LogInformation("Parsed {Count} navaids", navaids.Count);

        var routes = RouteParser.ParseRoutes(_document).ToList();
        _logger.LogInformation("Parsed {Count} routes", routes.Count);

        var segments = RouteSegmentParser.ParseRouteSegments(_document).ToList();
        _logger.LogInformation("Parsed {Count} route segments", segments.Count);

        // Link segments to routes
        LinkRouteSegments(routes, segments);

        // Calculate full path geometries for routes
        CalculateRouteGeometries(routes);

        var data = new AixmData
        {
            Airspaces = airspaces,
            DesignatedPoints = designatedPoints,
            AirportHeliports = airports,
            Navaids = navaids,
            Routes = routes
        };

        _logger.LogInformation("AIXM data parsing complete");
        return data;
    }

    /// <summary>
    /// Links route segments to their parent routes based on UUID references.
    /// </summary>
    private void LinkRouteSegments(List<Route> routes, List<RouteSegment> segments)
    {
        _logger.LogDebug("Linking route segments to routes");

        var routeLookup = routes.ToDictionary(
            r => UuidExtensions.NormalizeUuid(r.Uuid) ?? string.Empty,
            r => r,
            StringComparer.OrdinalIgnoreCase);

        int linkedCount = 0;
        foreach (var segment in segments)
        {
            if (!string.IsNullOrEmpty(segment.RouteRef))
            {
                var normalizedRef = UuidExtensions.NormalizeUuid(segment.RouteRef) ?? string.Empty;
                if (routeLookup.TryGetValue(normalizedRef, out var route))
                {
                    route.Segments.Add(segment);
                    linkedCount++;
                }
            }
        }

        _logger.LogInformation("Linked {LinkedCount} of {TotalCount} route segments to routes",
            linkedCount, segments.Count);
    }

    /// <summary>
    /// Calculates full path geometries and total lengths for routes from their segments.
    /// </summary>
    private void CalculateRouteGeometries(List<Route> routes)
    {
        _logger.LogDebug("Calculating route geometries");

        foreach (var route in routes)
        {
            if (route.Segments.Count > 0)
            {
                // Combine all segment geometries into route full path
                var segmentGeometries = route.Segments
                    .Select(s => s.Geometry)
                    .Where(g => g != null)
                    .Cast<NetTopologySuite.Geometries.LineString>();

                route.FullPathGeometry = GeometryBuilder.CombineLineStrings(segmentGeometries);

                // Calculate total length in nautical miles
                var totalLength = route.Segments
                    .Where(s => s.Length.HasValue && s.LengthUom == "NM")
                    .Sum(s => s.Length!.Value);

                if (totalLength > 0)
                    route.TotalLength = totalLength;
            }
        }

        var routesWithGeometry = routes.Count(r => r.FullPathGeometry != null);
        _logger.LogInformation("Calculated geometries for {Count} of {Total} routes",
            routesWithGeometry, routes.Count);
    }
}
