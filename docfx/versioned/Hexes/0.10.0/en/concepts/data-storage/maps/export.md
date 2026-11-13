# Export

Export writes a hex map as a binary record through <xref:Akeldov.Math.Hexes.HexMapFile> or
extension methods for `BinaryWriter`. A record preserves dimensions, layout, and cell values;
spatial records also preserve the origin and hex radius. See [Import](import.md) for restoring
maps with `ReadBoolHexMap` and the other readers.

## Supported map types

All export overloads are named `Write`. They support Boolean, integer, and floating-point maps:

| Values | Topology-only source | Spatial source |
|---|---|---|
| `bool` | `IHexMap<bool>` | `ISpatialHexMap<bool>` |
| `int` | `IHexMap<int>` | `ISpatialHexMap<int>` |
| `float` | `IHexMap<float>` | `ISpatialHexMap<float>` |

These overloads are available on both `HexMapFile` and `BinaryWriter`. Arbitrary `HexMap<T>`
value types are not supported. Do not change the source map while writing it.

## Export a file

```csharp
using Akeldov.Math.Hexes;

var mask = new BoolHexMap(
    new HexMapTopology(3, 2, Layout.OddR),
    new[] { true, false, true, false, true, false });

HexMapFile.Write("mask.hmap", mask);
```

`Write` creates or overwrites the target file. Its parent directory must already exist, and
relative paths resolve against the process's current working directory. The file is flushed
and closed on success, and closed on failure. Writing is not atomic: a failure after opening
can leave a partial file. File-system exceptions propagate to the caller.

## Export to a stream

Use <xref:Akeldov.Math.Hexes.HexMapBinaryWriterExtensions> to write records to an existing
binary stream. This example exports a Boolean map to an in-memory byte array:

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
byte[] data = stream.ToArray();
```

The extension methods leave the writer and underlying stream open. They do not flush or
reposition the stream; the caller controls their lifetime and positioning. Non-seekable streams
are also supported. Each call writes one record, allowing several maps and other data in one
stream. See [Import from a stream](import.md#import-from-a-stream) for reading those records.

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
```

Passing the same object as `IHexMap<float>` selects topology-only export and omits its geometry.
Read that record with `ReadFloatHexMap`; use `ReadSpatialFloatHexMap` for the spatial record
written above. The same overload-selection rule applies to `BinaryWriter.Write`.

## Binary format

Records start with the `HMAP` signature and format version `1`, independent of the package
version. Values follow the header in row-major order, with X advancing first. Boolean values use
one byte per cell; integers and floats use four little-endian bytes. Floating-point values retain
their bit patterns. Spatial records include the origin coordinates and radius before the values.
The format has no compression or checksum.

See [Save and Load Hex Maps](../../../how-to-guides/maps-and-topology/save-and-load-hex-maps.md)
for the byte-level format, [Import](import.md) for reading and validation, or [Maps](../maps.md)
for map types.
