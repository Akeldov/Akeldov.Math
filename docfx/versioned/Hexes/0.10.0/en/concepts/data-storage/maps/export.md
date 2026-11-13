# Export

Hex maps support binary export and import through <xref:Akeldov.Math.Hexes.HexMapFile> and
extension methods for `BinaryWriter` and `BinaryReader`. A record preserves the map's dimensions,
layout, and cell values; spatial records also preserve the origin and hex radius. Reading creates
a new mutable map with independent storage owned by the caller.

## Supported map types

All export overloads are named `Write`. Choose the import method matching both the value type
and whether the record includes spatial geometry:

| Values | Topology-only import | Spatial import |
|---|---|---|
| `bool` | `ReadBoolHexMap` | `ReadSpatialBoolHexMap` |
| `int` | `ReadIntHexMap` | `ReadSpatialIntHexMap` |
| `float` | `ReadFloatHexMap` | `ReadSpatialFloatHexMap` |

These methods are available on both `HexMapFile` and `BinaryReader`. Arbitrary `HexMap<T>` value
types are not supported. Import does not convert value types or discard spatial geometry: a
reader for a different record kind throws `InvalidDataException`.

## Export and import a file

```csharp
using Akeldov.Math.Hexes;

var mask = new BoolHexMap(
    new HexMapTopology(3, 2, Layout.OddR),
    new[] { true, false, true, false, true, false });

HexMapFile.Write("mask.hmap", mask);
BoolHexMap restored = HexMapFile.ReadBoolHexMap(
    "mask.hmap", maxCellCount: 1_000_000);
```

`Write` creates or overwrites the target file. Its parent directory must already exist, and
relative paths resolve against the process's current working directory. File methods close the
file on success or failure. A file reader reads the first record and ignores trailing bytes.
Writing is not atomic: a failure after opening can leave a partial file.

## Export and import a stream

Use <xref:Akeldov.Math.Hexes.HexMapBinaryWriterExtensions> and
<xref:Akeldov.Math.Hexes.HexMapBinaryReaderExtensions> to store records in an existing binary
stream. This example writes and reads a Boolean map in memory:

```csharp
using System.IO;
using System.Text;
using Akeldov.Math.Hexes;

var mask = new BoolHexMap(
    new HexMapTopology(2, 1, Layout.OddR), new[] { true, false });

using var stream = new MemoryStream();
using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
writer.Write(mask);
writer.Flush();

stream.Position = 0;
using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
BoolHexMap restored = reader.ReadBoolHexMap(maxCellCount: 100);
```

The extensions leave the reader, writer, and underlying stream open. They do not flush or
reposition the stream; the caller controls their lifetime and positioning. Non-seekable streams
are also supported. Each successful read consumes exactly one record, allowing several maps
and other data in one stream. A failed read does not restore the previous position. Do not
change the source map while writing it.

## Preserve spatial geometry

Overload selection depends on the source's compile-time type. A concrete spatial map or an
`ISpatialHexMap<T>` selects the spatial `Write` overload and preserves origin and radius:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    new HexMapTopology(2, 2, Layout.EvenQ), new VectorXY(-10f, 20f), radius: 2f);
var heights = new SpatialFloatHexMap(geometry, new[] { 1f, 2f, 3f, 4f });

HexMapFile.Write("heights.hmap", heights);
SpatialFloatHexMap restored = HexMapFile.ReadSpatialFloatHexMap(
    "heights.hmap", maxCellCount: 1_000_000);
```

Passing the same object as `IHexMap<float>` selects topology-only export and omits its geometry.
Read that record with `ReadFloatHexMap`. The same rule applies to `BinaryWriter.Write`.

## Format and validation

Records start with the `HMAP` signature and format version `1`, independent of the package
version. Values follow the header in row-major order, with X advancing first. Boolean values use
one byte per cell; integers and floats use four little-endian bytes. Floating-point values retain
their bit patterns. Spatial records include the origin coordinates and radius before the values.
The format has no compression or checksum.

`maxCellCount` limits allocations and defaults to `int.MaxValue`; zero permits only empty maps.
Readers validate dimensions, layout, record kinds, and spatial geometry before allocating values.
Invalid records or exceeded limits cause `InvalidDataException`; truncated records cause
`EndOfStreamException`. A negative limit causes `ArgumentOutOfRangeException`, and file-system
exceptions propagate to the caller.

See [Save and Load Hex Maps](../../../how-to-guides/maps-and-topology/save-and-load-hex-maps.md)
for the byte-level format, or return to [Maps](../maps.md).
