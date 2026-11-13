# Import

Import restores a hex map from a binary record written by [Export](export.md). Use
<xref:Akeldov.Math.Hexes.HexMapFile> to read a file, or the
<xref:Akeldov.Math.Hexes.HexMapBinaryReaderExtensions> methods on `BinaryReader` to read from
an existing stream. Each successful call creates a new mutable map with independent storage
owned by the caller.

## Choose a reader

Choose the method matching both the record's value type and whether it contains spatial geometry:

| Values | Topology-only import | Spatial import |
|---|---|---|
| `bool` | `ReadBoolHexMap` | `ReadSpatialBoolHexMap` |
| `int` | `ReadIntHexMap` | `ReadSpatialIntHexMap` |
| `float` | `ReadFloatHexMap` | `ReadSpatialFloatHexMap` |

All six methods are available on both `HexMapFile` and `BinaryReader`. Topology-only readers
restore dimensions, layout, and cell values. Spatial readers also restore the origin and hex
radius. Import does not convert value types or discard geometry: using a reader for a different
record kind throws `InvalidDataException`. Arbitrary `HexMap<T>` value types are not supported.

The writer's overload determines the record kind. A spatial map exported through an
`IHexMap<T>` variable contains only topology and values, so it requires a topology-only reader.
See [Preserve spatial geometry](export.md#preserve-spatial-geometry).

## Import a file

Read the Boolean map saved as `mask.hmap` in the [Export](export.md) example:

```csharp
using Akeldov.Math.Hexes;

BoolHexMap mask = HexMapFile.ReadBoolHexMap(
    "mask.hmap", maxCellCount: 1_000_000);
```

Absolute and relative paths are supported; relative paths resolve against the process's current
working directory. File methods close the file on success or failure. They read the first record
and ignore trailing bytes. To read multiple records, use a `BinaryReader` instead.

## Import from a stream

The reader must be positioned at the beginning of the next `HMAP` record. This example reads
one Boolean map from a file stream while keeping the stream open when the reader is disposed:

```csharp
using System.IO;
using System.Text;
using Akeldov.Math.Hexes;

using var stream = File.OpenRead("mask.hmap");
using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
BoolHexMap mask = reader.ReadBoolHexMap(maxCellCount: 1_000_000);
```

The extension method leaves the reader and stream open. The caller owns their lifetime; here,
`using` disposes both at the end of the scope. A successful call consumes exactly one record,
leaving subsequent data available. Read multiple maps in their written order, selecting the
matching method for each record. Seekable and non-seekable streams are supported.

After writing to a seekable stream, flush the writer and move to the record's starting position
before reading it. If reading fails, the previous stream position is not restored.

## Import a spatial map

Read the spatial floating-point map saved as `heights.hmap` in the export example:

```csharp
using Akeldov.Math.Hexes;

SpatialFloatHexMap heights = HexMapFile.ReadSpatialFloatHexMap(
    "heights.hmap", maxCellCount: 1_000_000);
```

The restored map retains its topology, origin, radius, and values. You do not need to supply a
new `HexMapGeometry`. Floating-point values retain their bit patterns, including signed zero,
infinities, and NaN payloads. Empty maps retain their dimensions and, for spatial records, geometry.

## Limits and invalid data

`maxCellCount` is the maximum accepted number of cells, checked before allocating value storage.
It defaults to `int.MaxValue`; set a limit appropriate for the application. Zero accepts only
empty maps. Readers validate the `HMAP` signature, format version, record kinds, dimensions,
layout, and spatial geometry. Seekable streams are also checked for a complete payload before
allocating values.

| Exception | Cause |
|---|---|
| `InvalidDataException` | Invalid or unsupported record, mismatched map/value kind, invalid Boolean byte, invalid geometry, or exceeded cell-count limit |
| `EndOfStreamException` | Incomplete header or payload |
| `ArgumentOutOfRangeException` | Negative `maxCellCount` |
| `ArgumentNullException` | Null path or reader |

File-system exceptions propagate to the caller. The format has no checksum, so structural
validation does not detect every possible change to stored values.

See [Save and Load Hex Maps](../../../how-to-guides/maps-and-topology/save-and-load-hex-maps.md)
for the binary format, [Export](export.md) for writing records, or [Maps](../maps.md) for map types.
