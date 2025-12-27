using AixmParser.Core.Models;
using AixmParser.GeoJson.Exporters;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace AixmParser.GeoJson;

/// <summary>
/// Main API for exporting AIXM data to GeoJSON format.
/// Provides methods to export individual feature types or all features at once.
/// </summary>
public static class GeoJsonExporter
{
    private static readonly JsonSerializerSettings DefaultSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    #region Export to String

    /// <summary>
    /// Exports airspaces to a GeoJSON string.
    /// </summary>
    /// <param name="airspaces">List of airspaces to export.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    /// <returns>GeoJSON string representation.</returns>
    public static string ExportAirspaces(IEnumerable<Airspace> airspaces, Formatting formatting = Formatting.Indented)
    {
        var collection = AirspaceExporter.ToFeatureCollection(airspaces);
        return SerializeFeatureCollection(collection, formatting);
    }

    /// <summary>
    /// Exports routes to a GeoJSON string.
    /// </summary>
    /// <param name="routes">List of routes to export.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    /// <returns>GeoJSON string representation.</returns>
    public static string ExportRoutes(IEnumerable<Route> routes, Formatting formatting = Formatting.Indented)
    {
        var collection = RouteExporter.ToFeatureCollection(routes);
        return SerializeFeatureCollection(collection, formatting);
    }

    /// <summary>
    /// Exports route segments to a GeoJSON string.
    /// </summary>
    /// <param name="routes">List of routes containing segments to export.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    /// <returns>GeoJSON string representation.</returns>
    public static string ExportRouteSegments(IEnumerable<Route> routes, Formatting formatting = Formatting.Indented)
    {
        var collection = RouteSegmentExporter.ToFeatureCollection(routes);
        return SerializeFeatureCollection(collection, formatting);
    }

    /// <summary>
    /// Exports navaids to a GeoJSON string.
    /// </summary>
    /// <param name="navaids">List of navaids to export.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    /// <returns>GeoJSON string representation.</returns>
    public static string ExportNavaids(IEnumerable<Navaid> navaids, Formatting formatting = Formatting.Indented)
    {
        var collection = NavaidExporter.ToFeatureCollection(navaids);
        return SerializeFeatureCollection(collection, formatting);
    }

    /// <summary>
    /// Exports designated points to a GeoJSON string.
    /// </summary>
    /// <param name="points">List of designated points to export.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    /// <returns>GeoJSON string representation.</returns>
    public static string ExportDesignatedPoints(IEnumerable<DesignatedPoint> points, Formatting formatting = Formatting.Indented)
    {
        var collection = DesignatedPointExporter.ToFeatureCollection(points);
        return SerializeFeatureCollection(collection, formatting);
    }

    /// <summary>
    /// Exports airports/heliports to a GeoJSON string.
    /// </summary>
    /// <param name="airports">List of airports/heliports to export.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    /// <returns>GeoJSON string representation.</returns>
    public static string ExportAirports(IEnumerable<AirportHeliport> airports, Formatting formatting = Formatting.Indented)
    {
        var collection = AirportHeliportExporter.ToFeatureCollection(airports);
        return SerializeFeatureCollection(collection, formatting);
    }

    #endregion

    #region Export to File

    /// <summary>
    /// Exports airspaces to a GeoJSON file.
    /// </summary>
    /// <param name="airspaces">List of airspaces to export.</param>
    /// <param name="filePath">Output file path.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    public static void ExportAirspacesToFile(IEnumerable<Airspace> airspaces, string filePath, Formatting formatting = Formatting.Indented)
    {
        var geojson = ExportAirspaces(airspaces, formatting);
        File.WriteAllText(filePath, geojson);
    }

    /// <summary>
    /// Exports routes to a GeoJSON file.
    /// </summary>
    /// <param name="routes">List of routes to export.</param>
    /// <param name="filePath">Output file path.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    public static void ExportRoutesToFile(IEnumerable<Route> routes, string filePath, Formatting formatting = Formatting.Indented)
    {
        var geojson = ExportRoutes(routes, formatting);
        File.WriteAllText(filePath, geojson);
    }

    /// <summary>
    /// Exports route segments to a GeoJSON file.
    /// </summary>
    /// <param name="routes">List of routes containing segments to export.</param>
    /// <param name="filePath">Output file path.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    public static void ExportRouteSegmentsToFile(IEnumerable<Route> routes, string filePath, Formatting formatting = Formatting.Indented)
    {
        var geojson = ExportRouteSegments(routes, formatting);
        File.WriteAllText(filePath, geojson);
    }

    /// <summary>
    /// Exports navaids to a GeoJSON file.
    /// </summary>
    /// <param name="navaids">List of navaids to export.</param>
    /// <param name="filePath">Output file path.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    public static void ExportNavaidsToFile(IEnumerable<Navaid> navaids, string filePath, Formatting formatting = Formatting.Indented)
    {
        var geojson = ExportNavaids(navaids, formatting);
        File.WriteAllText(filePath, geojson);
    }

    /// <summary>
    /// Exports designated points to a GeoJSON file.
    /// </summary>
    /// <param name="points">List of designated points to export.</param>
    /// <param name="filePath">Output file path.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    public static void ExportDesignatedPointsToFile(IEnumerable<DesignatedPoint> points, string filePath, Formatting formatting = Formatting.Indented)
    {
        var geojson = ExportDesignatedPoints(points, formatting);
        File.WriteAllText(filePath, geojson);
    }

    /// <summary>
    /// Exports airports/heliports to a GeoJSON file.
    /// </summary>
    /// <param name="airports">List of airports/heliports to export.</param>
    /// <param name="filePath">Output file path.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    public static void ExportAirportsToFile(IEnumerable<AirportHeliport> airports, string filePath, Formatting formatting = Formatting.Indented)
    {
        var geojson = ExportAirports(airports, formatting);
        File.WriteAllText(filePath, geojson);
    }

    /// <summary>
    /// Exports all AIXM data to separate GeoJSON files in the specified directory.
    /// </summary>
    /// <param name="data">The AIXM data to export.</param>
    /// <param name="outputDirectory">Directory where files will be created.</param>
    /// <param name="formatting">JSON formatting (Indented or None).</param>
    public static void ExportAllToDirectory(AixmData data, string outputDirectory, Formatting formatting = Formatting.Indented)
    {
        Directory.CreateDirectory(outputDirectory);

        ExportAirspacesToFile(data.Airspaces, Path.Combine(outputDirectory, "airspaces.geojson"), formatting);
        ExportRoutesToFile(data.Routes, Path.Combine(outputDirectory, "routes.geojson"), formatting);
        ExportRouteSegmentsToFile(data.Routes, Path.Combine(outputDirectory, "route_segments.geojson"), formatting);
        ExportNavaidsToFile(data.Navaids, Path.Combine(outputDirectory, "navaids.geojson"), formatting);
        ExportDesignatedPointsToFile(data.DesignatedPoints, Path.Combine(outputDirectory, "designated_points.geojson"), formatting);
        ExportAirportsToFile(data.AirportHeliports, Path.Combine(outputDirectory, "airports.geojson"), formatting);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Serializes a FeatureCollection to GeoJSON string.
    /// </summary>
    private static string SerializeFeatureCollection(FeatureCollection collection, Formatting formatting)
    {
        var serializer = GeoJsonSerializer.Create();
        using var stringWriter = new StringWriter();
        using var jsonWriter = new JsonTextWriter(stringWriter)
        {
            Formatting = formatting
        };

        serializer.Serialize(jsonWriter, collection);
        return stringWriter.ToString();
    }

    #endregion
}
