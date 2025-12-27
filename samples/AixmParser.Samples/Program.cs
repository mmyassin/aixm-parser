using AixmParser.Core;
using AixmParser.GeoJson;

Console.WriteLine("AIXM Parser - Sample Application");
Console.WriteLine("=================================\n");

// Path to the Oman AIXM file from the old project
string aixmFilePath = @"path_to_aixm_file.xml";
string outputDirectory = @"path_output\geojson";

if (!File.Exists(aixmFilePath))
{
    Console.WriteLine($"Error: AIXM file not found at: {aixmFilePath}");
    Console.WriteLine("Please update the path in Program.cs");
    return;
}

try
{
    Console.WriteLine($"Loading AIXM file: {Path.GetFileName(aixmFilePath)}");
    Console.WriteLine("This may take a moment...\n");

    var aixm = AixmDocument.Load(aixmFilePath);
    var data = aixm.Data;

    Console.WriteLine("Parsing complete! Here's what was found:");
    Console.WriteLine($"  Airspaces:         {data.Airspaces.Count,5}");
    Console.WriteLine($"  Routes:            {data.Routes.Count,5}");
    Console.WriteLine($"  Route Segments:    {data.Routes.Sum(r => r.Segments.Count),5}");
    Console.WriteLine($"  Navaids:           {data.Navaids.Count,5}");
    Console.WriteLine($"  Designated Points: {data.DesignatedPoints.Count,5}");
    Console.WriteLine($"  Airports:          {data.AirportHeliports.Count,5}");

    Console.WriteLine("\n--- Sample Data ---");

    // Show first airspace
    var firstAirspace = data.Airspaces.FirstOrDefault();
    if (firstAirspace != null)
    {
        Console.WriteLine($"\nFirst Airspace:");
        Console.WriteLine($"  UUID:       {firstAirspace.Uuid}");
        Console.WriteLine($"  Designator: {firstAirspace.Designator}");
        Console.WriteLine($"  Type:       {firstAirspace.Type}");
        Console.WriteLine($"  Name:       {firstAirspace.Name}");
        Console.WriteLine($"  Lower:      {firstAirspace.LowerLimit} {firstAirspace.LowerLimitReference}");
        Console.WriteLine($"  Upper:      {firstAirspace.UpperLimit} {firstAirspace.UpperLimitReference}");
        Console.WriteLine($"  Has Geometry: {firstAirspace.Geometry != null}");
    }

    // Show first route with segments
    var routeWithSegments = data.Routes.FirstOrDefault(r => r.Segments.Count > 0);
    if (routeWithSegments != null)
    {
        Console.WriteLine($"\nFirst Route (with segments):");
        Console.WriteLine($"  Designator:    {routeWithSegments.Designator}");
        Console.WriteLine($"  Segments:      {routeWithSegments.Segments.Count}");
        Console.WriteLine($"  Total Length:  {routeWithSegments.TotalLength:F2} NM");
        Console.WriteLine($"  Has Geometry:  {routeWithSegments.FullPathGeometry != null}");

        if (routeWithSegments.Segments.Count > 0)
        {
            var seg = routeWithSegments.Segments[0];
            Console.WriteLine($"  First Segment:");
            Console.WriteLine($"    Start:  {seg.StartPointRef}");
            Console.WriteLine($"    End:    {seg.EndPointRef}");
            Console.WriteLine($"    Length: {seg.Length} {seg.LengthUom}");
        }
    }

    // Show first navaid
    var firstNavaid = data.Navaids.FirstOrDefault();
    if (firstNavaid != null)
    {
        Console.WriteLine($"\nFirst Navaid:");
        Console.WriteLine($"  Designator: {firstNavaid.Designator}");
        Console.WriteLine($"  Type:       {firstNavaid.Type}");
        Console.WriteLine($"  Name:       {firstNavaid.Name}");
        Console.WriteLine($"  Position:   {firstNavaid.Latitude:F4}, {firstNavaid.Longitude:F4}");
        if (firstNavaid.Frequency.HasValue)
            Console.WriteLine($"  Frequency:  {firstNavaid.Frequency} {firstNavaid.FrequencyUom}");
    }

    Console.WriteLine("\n✓ All data parsed successfully!");

    // Export to GeoJSON
    Console.WriteLine("\n--- Exporting to GeoJSON ---");
    Console.WriteLine($"Output directory: {outputDirectory}\n");

    GeoJsonExporter.ExportAllToDirectory(data, outputDirectory);

    Console.WriteLine($"✓ Exported airspaces to:         {Path.Combine(outputDirectory, "airspaces.geojson")}");
    Console.WriteLine($"✓ Exported routes to:            {Path.Combine(outputDirectory, "routes.geojson")}");
    Console.WriteLine($"✓ Exported route segments to:    {Path.Combine(outputDirectory, "route_segments.geojson")}");
    Console.WriteLine($"✓ Exported navaids to:           {Path.Combine(outputDirectory, "navaids.geojson")}");
    Console.WriteLine($"✓ Exported designated points to: {Path.Combine(outputDirectory, "designated_points.geojson")}");
    Console.WriteLine($"✓ Exported airports to:          {Path.Combine(outputDirectory, "airports.geojson")}");

    Console.WriteLine("\n✓ GeoJSON export complete!");

    // Show sample GeoJSON output
    Console.WriteLine("\n--- Sample GeoJSON Output (First Navaid) ---");
    var navaidGeoJson = GeoJsonExporter.ExportNavaids(data.Navaids.Take(1));
    Console.WriteLine(navaidGeoJson.Substring(0, Math.Min(500, navaidGeoJson.Length)) + "...");
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
