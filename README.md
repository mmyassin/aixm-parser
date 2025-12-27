# AixmParser

A robust .NET library suite for parsing Aeronautical Information Exchange Model (AIXM) 5.1 and 5.1.1 XML data. Parse AIXM files and export to NetTopologySuite geometries or GeoJSON format.

## Packages

- **AixmParser.Core**: Core parsing library that returns NetTopologySuite geometries
- **AixmParser.GeoJson**: GeoJSON export functionality for AIXM data

## Features

- **Full AIXM 5.1/5.1.1 Support**: Parse all major aeronautical features
- **NetTopologySuite Integration**: Returns spatial geometries (Point, LineString, Polygon, etc.)
- **GeoJSON Export**: Convert AIXM data to GeoJSON format with full FeatureCollection support
- **Clean Architecture**: Separated concerns with dedicated parsers for each feature type
- **High Performance**: Efficient XML parsing with lazy loading
- **Type-Safe**: Fully typed models with nullable reference types
- **Well Documented**: Comprehensive XML documentation for all public APIs

## Supported AIXM Features

- **Airspaces**: Controlled airspace volumes with vertical limits and boundaries
- **Routes**: Air traffic service routes with segments
- **Route Segments**: Individual segments connecting waypoints
- **Navaids**: Navigation aids (VOR, DME, NDB, TACAN, etc.)
- **Designated Points**: Published waypoints
- **Airport/Heliport**: Aerodrome facilities with reference points

## Installation

### AixmParser.Core

```bash
dotnet add package AixmParser.Core
```

### AixmParser.GeoJson

```bash
dotnet add package AixmParser.GeoJson
```

## Quick Start

```csharp
using AixmParser.Core;

// Load AIXM file
var aixm = AixmDocument.Load("path/to/aixm-file.xml");

// Access parsed data
var data = aixm.Data;

Console.WriteLine($"Found {data.Airspaces.Count} airspaces");
Console.WriteLine($"Found {data.Routes.Count} routes");
Console.WriteLine($"Found {data.Navaids.Count} navaids");
Console.WriteLine($"Found {data.DesignatedPoints.Count} designated points");
Console.WriteLine($"Found {data.AirportHeliports.Count} airports");

// Work with geometries
foreach (var airspace in data.Airspaces.Where(a => a.Geometry != null))
{
    var geometry = airspace.Geometry; // NetTopologySuite.Geometries.Geometry
    var area = geometry.Area;
    var bounds = geometry.EnvelopeInternal;

    Console.WriteLine($"{airspace.Designator}: {airspace.Type}");
    Console.WriteLine($"  Bounds: {bounds}");
}

// Examine route segments
foreach (var route in data.Routes.Where(r => r.Segments.Count > 0))
{
    Console.WriteLine($"Route {route.Designator}:");
    Console.WriteLine($"  Segments: {route.Segments.Count}");
    Console.WriteLine($"  Total Length: {route.TotalLength:F2} NM");
    Console.WriteLine($"  Has Geometry: {route.FullPathGeometry != null}");
}
```

## Exporting to GeoJSON

Convert AIXM data to GeoJSON format:

```csharp
using AixmParser.Core;
using AixmParser.GeoJson;

// Load AIXM file
var aixm = AixmDocument.Load("path/to/aixm-file.xml");

// Export all features to GeoJSON
var geoJson = aixm.Data.ToGeoJson();

// Save to file
File.WriteAllText("output.geojson", geoJson);

// Export specific feature types
var airspacesGeoJson = aixm.Data.Airspaces.ToGeoJson();
var routesGeoJson = aixm.Data.Routes.ToGeoJson();
var navaidsGeoJson = aixm.Data.Navaids.ToGeoJson();
```

## Working with Geometries

All geometries are returned as NetTopologySuite types:

```csharp
using NetTopologySuite.Geometries;

// Airspace geometries (Polygon or MultiPolygon)
Geometry airspaceGeom = airspace.Geometry;
double area = airspaceGeom.Area;

// Navaid/Airport point geometries
Point navaidPoint = navaid.Geometry;
double lat = navaidPoint.Y;
double lon = navaidPoint.X;
double elevation = navaidPoint.Z; // if available

// Route geometries (LineString or MultiLineString)
Geometry routeGeom = route.FullPathGeometry;
double length = routeGeom.Length;
```

## Advanced Usage

### Using with Logging

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var aixm = AixmDocument.Load("path/to/file.xml", logger);
```

### Loading from Stream

```csharp
using var stream = File.OpenRead("path/to/file.xml");
var aixm = AixmDocument.Load(stream);
```

### Loading from XDocument

```csharp
using System.Xml.Linq;

var doc = XDocument.Load("path/to/file.xml");
var aixm = AixmDocument.Parse(doc);
```

## Data Models

### Airspace

```csharp
public class Airspace
{
    public string? Uuid { get; set; }
    public string? Designator { get; set; }
    public string? Type { get; set; }  // CTR, TMA, FIR, RESTRICTED, etc.
    public string? Name { get; set; }
    public string? LowerLimit { get; set; }
    public string? LowerLimitReference { get; set; }  // SFC, MSL, AGL
    public string? UpperLimit { get; set; }
    public string? UpperLimitReference { get; set; }  // MSL, FL
    public Geometry? Geometry { get; set; }  // Polygon or MultiPolygon
}
```

### Route

```csharp
public class Route
{
    public string? Uuid { get; set; }
    public string? Designator { get; set; }
    public string? LocationDesignator { get; set; }
    public double? TotalLength { get; set; }  // in nautical miles
    public List<RouteSegment> Segments { get; set; }
    public Geometry? FullPathGeometry { get; set; }  // LineString or MultiLineString
}
```

### Navaid

```csharp
public class Navaid
{
    public string? Uuid { get; set; }
    public string? Type { get; set; }  // VOR, DME, NDB, TACAN, etc.
    public string? Designator { get; set; }
    public string? Name { get; set; }
    public double? Frequency { get; set; }
    public string? FrequencyUom { get; set; }
    public string? Channel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Elevation { get; set; }
    public Point? Geometry { get; set; }
}
```

## Architecture

The library suite is organized into clean, focused modules:

```
AixmParser/
├── AixmParser.Core/
│   ├── Models/              # Data models for AIXM features
│   ├── Parsers/             # Feature-specific parsers
│   ├── GeometryParsing/     # Geometry parsing utilities
│   ├── Common/              # Shared utilities and extensions
│   └── AixmDocument.cs      # Main API entry point
│
└── AixmParser.GeoJson/
    ├── Converters/          # GeoJSON conversion logic
    └── Extensions/          # Extension methods for easy export
```

## Performance Considerations

- **Lazy Loading**: Data is only parsed when accessing the `Data` property
- **Streaming**: Supports loading from streams for large files
- **Memory Efficient**: Uses LINQ deferred execution where possible

## Dependencies

- **.NET 6.0** or higher
- **NetTopologySuite 2.5.0**: Spatial geometry library
- **Microsoft.Extensions.Logging.Abstractions 8.0.0**: Logging support

## Building from Source

```bash
git clone https://github.com/mmyassin/aixm-parser.git
cd aixm-parser
dotnet build
dotnet test
```

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Roadmap

- [ ] Support for additional AIXM features (Runways, Obstacles, etc.)
- [ ] Performance optimizations for very large files
- [ ] XML validation against AIXM schema

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/mmyassin/aixm-parser).
