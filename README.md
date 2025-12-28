# AixmParser

[![.NET](https://img.shields.io/badge/.NET-6.0+-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-Coming%20Soon-blue)](https://www.nuget.org/)

A powerful and intuitive .NET library suite for parsing Aeronautical Information Exchange Model (AIXM) 5.1/5.1.1 XML data. Seamlessly convert aviation data into NetTopologySuite geometries or GeoJSON format for mapping and analysis.

<img width="734" height="540" alt="AIXM Parser Visualization" src="https://github.com/user-attachments/assets/6854e412-f234-4a3e-9a65-a6405b1214a0" />

---

## 📦 Packages

| Package | Description | Status |
|---------|-------------|--------|
| **AixmParser.Core** | Core parsing engine with NetTopologySuite geometry support | ✅ Available |
| **AixmParser.GeoJson** | GeoJSON export functionality for seamless web mapping | ✅ Available |

---

## ✨ Features

### 🎯 Core Capabilities
- **Complete AIXM 5.1/5.1.1 Support** – Parse all major aeronautical features including airspaces, routes, navaids, and more
- **NetTopologySuite Integration** – Native spatial geometry support (Point, LineString, Polygon, MultiPolygon)
- **GeoJSON Export** – One-click conversion to GeoJSON with full FeatureCollection support
- **Multi-CRS Support** – Automatic coordinate system detection and conversion (EPSG:4326, CRS84, etc.)

### 🏗️ Architecture & Design
- **Clean Architecture** – Separation of concerns with dedicated parsers for each feature type
- **Type-Safe Models** – Fully typed with nullable reference types for compile-time safety
- **High Performance** – Efficient XML parsing with lazy loading and streaming support
- **Extensible** – Easy to extend for additional AIXM features or custom formats

### 📚 Developer Experience
- **Well Documented** – Comprehensive XML documentation for IntelliSense support
- **Easy Onboarding** – Simple API with minimal configuration required
- **Logging Support** – Built-in Microsoft.Extensions.Logging integration
- **Battle-Tested** – Validated against real-world AIXM datasets from multiple countries

---

## 🗺️ Supported AIXM Features

| Feature | Description | Geometry Type |
|---------|-------------|---------------|
| **Airspaces** | Controlled airspace volumes (CTR, TMA, FIR, Restricted areas) | Polygon/MultiPolygon |
| **Routes** | Air traffic service routes with waypoint segments | LineString/MultiLineString |
| **Route Segments** | Individual segments connecting designated points | LineString |
| **Navaids** | Navigation aids (VOR, DME, NDB, TACAN, VOR/DME, etc.) | Point |
| **Designated Points** | Published waypoints and fixes | Point |
| **Airports/Heliports** | Aerodrome facilities with reference points | Point |

---

## 🚀 Quick Start

### Installation

```bash
# Core parsing library
dotnet add package AixmParser.Core

# GeoJSON export (optional)
dotnet add package AixmParser.GeoJson
```

### Basic Usage

```csharp
using AixmParser.Core;

// Load and parse AIXM file
var aixm = AixmDocument.Load("path/to/aixm-file.xml");
var data = aixm.Data;

// Inspect parsed data
Console.WriteLine($"📍 {data.Airspaces.Count} airspaces");
Console.WriteLine($"🛣️  {data.Routes.Count} routes");
Console.WriteLine($"📡 {data.Navaids.Count} navaids");
Console.WriteLine($"📌 {data.DesignatedPoints.Count} designated points");
Console.WriteLine($"✈️  {data.AirportHeliports.Count} airports");

// Work with geometries
foreach (var airspace in data.Airspaces.Where(a => a.Geometry != null))
{
    var bounds = airspace.Geometry.EnvelopeInternal;
    Console.WriteLine($"{airspace.Designator} ({airspace.Type})");
    Console.WriteLine($"  Bounds: {bounds}");
    Console.WriteLine($"  Limits: {airspace.LowerLimit} - {airspace.UpperLimit}");
}

// Analyze routes
foreach (var route in data.Routes.Where(r => r.Segments.Count > 0))
{
    Console.WriteLine($"Route {route.Designator}:");
    Console.WriteLine($"  ├─ {route.Segments.Count} segments");
    Console.WriteLine($"  ├─ {route.TotalLength:F2} NM total length");
    Console.WriteLine($"  └─ Geometry: {(route.FullPathGeometry != null ? "✓" : "✗")}");
}
```

### Export to GeoJSON

```csharp
using AixmParser.Core;
using AixmParser.GeoJson;

// Load AIXM file
var aixm = AixmDocument.Load("spain-enroute.xml");

// Export everything to a single directory
GeoJsonExporter.ExportAllToDirectory(
    aixm.Data,
    outputDirectory: "output"
);

// Or export specific features
var airspacesJson = GeoJsonExporter.ExportAirspaces(aixm.Data.Airspaces);
File.WriteAllText("airspaces.geojson", airspacesJson);

var routesJson = GeoJsonExporter.ExportRoutes(aixm.Data.Routes);
File.WriteAllText("routes.geojson", routesJson);
```

**Generated files:**
- `airspaces.geojson` – All airspace volumes
- `routes.geojson` – Complete route paths
- `route_segments.geojson` – Individual route segments
- `navaids.geojson` – Navigation aids
- `designated_points.geojson` – Waypoints and fixes
- `airports.geojson` – Airport reference points

---

## 📖 Documentation

### Working with Geometries

All geometries use [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) – the industry-standard .NET spatial library:

```csharp
using NetTopologySuite.Geometries;

// Airspace geometries (Polygon or MultiPolygon)
Geometry airspaceGeom = airspace.Geometry;
double areaInSquareDegrees = airspaceGeom.Area;
Envelope bounds = airspaceGeom.EnvelopeInternal;

// Navaid point geometries (Point)
Point navaidPoint = navaid.Geometry;
double latitude = navaidPoint.Y;   // NTS uses Y for latitude
double longitude = navaidPoint.X;  // NTS uses X for longitude
double elevation = navaidPoint.Z;  // Z coordinate if available

// Route geometries (LineString or MultiLineString)
Geometry routeGeom = route.FullPathGeometry;
double lengthInDegrees = routeGeom.Length;
Coordinate[] waypoints = routeGeom.Coordinates;
```

### Loading Options

```csharp
// From file path (recommended)
var aixm = AixmDocument.Load("path/to/file.xml");

// From stream (for large files or network sources)
using var stream = File.OpenRead("path/to/file.xml");
var aixm = AixmDocument.Load(stream);

// From XDocument (for pre-processing)
using System.Xml.Linq;
var doc = XDocument.Load("path/to/file.xml");
var aixm = AixmDocument.Parse(doc);

// With logging support
using Microsoft.Extensions.Logging;
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();
var aixm = AixmDocument.Load("path/to/file.xml", logger);
```

### Data Models

<details>
<summary><b>Airspace</b> – Controlled airspace volumes</summary>

```csharp
public class Airspace
{
    public string? Uuid { get; set; }
    public string? Designator { get; set; }
    public string? Type { get; set; }           // CTR, TMA, FIR, RESTRICTED, DANGER, etc.
    public string? Name { get; set; }
    public string? LowerLimit { get; set; }
    public string? LowerLimitReference { get; set; }  // SFC, MSL, AGL
    public string? UpperLimit { get; set; }
    public string? UpperLimitReference { get; set; }  // MSL, FL (Flight Level)
    public Geometry? Geometry { get; set; }     // Polygon or MultiPolygon
}
```
</details>

<details>
<summary><b>Route</b> – Air traffic service routes</summary>

```csharp
public class Route
{
    public string? Uuid { get; set; }
    public string? Designator { get; set; }      // e.g., "UN871", "Y27"
    public string? LocationDesignator { get; set; }
    public double? TotalLength { get; set; }     // Nautical miles
    public List<RouteSegment> Segments { get; set; }
    public Geometry? FullPathGeometry { get; set; }  // LineString or MultiLineString
}
```
</details>

<details>
<summary><b>RouteSegment</b> – Individual route segment</summary>

```csharp
public class RouteSegment
{
    public string? Uuid { get; set; }
    public string? RouteRef { get; set; }        // Parent route UUID
    public string? StartPointRef { get; set; }   // Starting waypoint UUID
    public string? EndPointRef { get; set; }     // Ending waypoint UUID
    public double? Length { get; set; }
    public string? LengthUom { get; set; }       // Usually "NM" (nautical miles)
    public double? TrueTrack { get; set; }       // True bearing in degrees
    public double? MagneticTrack { get; set; }   // Magnetic bearing in degrees
    public string? LowerLimit { get; set; }
    public string? UpperLimit { get; set; }
    public LineString? Geometry { get; set; }
}
```
</details>

<details>
<summary><b>Navaid</b> – Navigation aids</summary>

```csharp
public class Navaid
{
    public string? Uuid { get; set; }
    public string? Type { get; set; }            // VOR, DME, NDB, TACAN, VOR_DME, etc.
    public string? Designator { get; set; }      // e.g., "DXB", "VGE"
    public string? Name { get; set; }
    public double? Frequency { get; set; }
    public string? FrequencyUom { get; set; }    // "MHZ" or "KHZ"
    public string? Channel { get; set; }         // DME/TACAN channel
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Elevation { get; set; }
    public string? ElevationUom { get; set; }    // "M" or "FT"
    public Point? Geometry { get; set; }
}
```
</details>

<details>
<summary><b>DesignatedPoint</b> – Published waypoints</summary>

```csharp
public class DesignatedPoint
{
    public string? Uuid { get; set; }
    public string? Designator { get; set; }      // 5-letter waypoint name
    public string? Name { get; set; }
    public string? Type { get; set; }            // ICAO, ADHP, etc.
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Elevation { get; set; }
    public Point? Geometry { get; set; }
}
```
</details>

<details>
<summary><b>AirportHeliport</b> – Aerodrome facilities</summary>

```csharp
public class AirportHeliport
{
    public string? Uuid { get; set; }
    public string? Designator { get; set; }      // ICAO code (e.g., "OMDB")
    public string? IataDesignator { get; set; }  // IATA code (e.g., "DXB")
    public string? Name { get; set; }
    public string? Type { get; set; }            // AD (Airport), HP (Heliport)
    public double? Latitude { get; set; }        // ARP (Aerodrome Reference Point)
    public double? Longitude { get; set; }
    public double? Elevation { get; set; }
    public string? ElevationUom { get; set; }
    public Point? Geometry { get; set; }
}
```
</details>

---

## 🏗️ Project Structure

```
AixmParser/
├── src/
│   ├── AixmParser.Core/
│   │   ├── Models/              # Data models for AIXM features
│   │   ├── Parsers/             # Feature-specific parsers
│   │   ├── GeometryParsing/     # Coordinate & geometry parsing
│   │   ├── Common/              # Shared utilities & extensions
│   │   └── AixmDocument.cs      # Main API entry point
│   │
│   └── AixmParser.GeoJson/
│       ├── Exporters/           # GeoJSON conversion logic
│       └── GeoJsonExporter.cs   # Public export API
│
├── samples/
│   └── AixmParser.Samples/      # Example usage
│
└── tests/                       # Unit tests (coming soon)
```

---

## ⚡ Performance & Best Practices

### Memory Efficiency
- **Lazy Loading**: Data is only parsed when accessing the `Data` property
- **Streaming Support**: Load from streams for large files without reading entire file into memory
- **Deferred Execution**: LINQ queries are evaluated on-demand

### Coordinate System Handling
The parser automatically detects and handles different coordinate reference systems:
- **EPSG:4326**: Lat,Lon order → converted to Lon,Lat for NTS/GeoJSON
- **CRS84**: Lon,Lat order → used directly
- **Automatic Detection**: Based on `srsName` attribute in AIXM data

### Large File Tips
```csharp
// For very large files, use streaming
using var stream = new FileStream("large-file.xml",
    FileMode.Open, FileAccess.Read, FileShare.Read,
    bufferSize: 65536, useAsync: true);
var aixm = AixmDocument.Load(stream);

// Process features in batches
foreach (var batch in aixm.Data.Airspaces.Chunk(100))
{
    // Process batch...
}
```

---

## 🔧 Building from Source

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or higher
- Git

### Clone and Build
```bash
git clone https://github.com/mmyassin/aixm-parser.git
cd aixm-parser
dotnet restore
dotnet build
```

### Run Samples
```bash
cd samples/AixmParser.Samples
dotnet run
```

### Run Tests (Coming Soon)
```bash
dotnet test
```

---

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| NetTopologySuite | 2.5.0+ | Spatial geometry support |
| NetTopologySuite.IO.GeoJSON | 4.0.0+ | GeoJSON serialization |
| Microsoft.Extensions.Logging.Abstractions | 8.0.0+ | Logging interface |
| Newtonsoft.Json | 13.0.0+ | JSON serialization |

---

## 🗺️ Roadmap

### Upcoming Features
- [ ] **Additional AIXM Features**
  - [ ] Runways and taxiways
  - [ ] Obstacles and terrain
  - [ ] Airspace activation schedules
  - [ ] Procedure data (SID/STAR/Approach)
- [ ] **Performance Enhancements**
  - [ ] Parallel parsing for very large files
  - [ ] Memory-mapped file support
- [ ] **Validation & Quality**
  - [ ] XML schema validation against AIXM XSD
  - [ ] Geometry validation and repair
  - [ ] Comprehensive unit test suite
- [ ] **Export Formats**
  - [ ] KML/KMZ export
  - [ ] Shapefile export
  - [ ] SQLite/GeoPackage export

### Long-term Vision
- Support for AIXM 5.2 and future versions
- Real-time AIXM data streaming
- Integration with popular GIS frameworks

---

## 🤝 Contributing

Contributions are welcome! Whether it's bug reports, feature requests, or pull requests, all contributions help make this library better.

### How to Contribute
1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Development Guidelines
- Follow existing code style and conventions
- Add XML documentation for public APIs
- Include examples for new features
- Ensure backward compatibility when possible

---

## 🆘 Support & Community

- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/mmyassin/aixm-parser/issues)
- 💡 **Feature Requests**: [GitHub Discussions](https://github.com/mmyassin/aixm-parser/discussions)
- 📧 **Contact**: Create an issue for questions and support

---

## 🙏 Acknowledgments

- **EUROCONTROL** for the AIXM standard specification
- **NetTopologySuite** team for the excellent spatial library
- All contributors and users providing feedback and real-world datasets

---

<div align="center">

**Made with ❤️ for the aviation community**

⭐ **Star this repo** if you find it useful!

</div>
