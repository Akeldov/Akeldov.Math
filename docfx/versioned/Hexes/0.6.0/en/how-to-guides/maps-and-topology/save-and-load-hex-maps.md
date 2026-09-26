# Save and Load Hex Maps

Hexes 0.6.0 adds binary serialization for Boolean, integer, and floating-point maps, including
their spatial variants. The file methods create independent mutable maps when reading.

## Save a spatial map to a file

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    new HexMapTopology(2, 2, Layout.EvenQ), new VectorXY(-10f, 20f), radius: 2f);
var map = new SpatialFloatHexMap(geometry, new[] { 1.5f, -2.25f, 0f, float.NaN });

HexMapFile.Write("terrain.hmap", map);
SpatialFloatHexMap restored = HexMapFile.ReadSpatialFloatHexMap(
    "terrain.hmap", maxCellCount: 1_000_000);
```

Absolute and relative paths are supported. Relative paths resolve against the process's current
working directory. Parent directories must exist. Writing creates or overwrites the file;
it is not atomic, so an error after opening can leave a partial record. Files are closed on
success or failure. File readers consume the first record and ignore trailing data.

## Choose the map type

All six writers are named `Write`. Overload selection uses the compile-time source type.
Concrete spatial maps and `ISpatialHexMap<T>` preserve topology, origin, radius, and values.
An `IHexMap<T>` variable selects topology-only serialization, even when its object is spatial.

| Value type | Topology-only reader | Spatial reader |
|---|---|---|
| `bool` | `ReadBoolHexMap` | `ReadSpatialBoolHexMap` |
| `int` | `ReadIntHexMap` | `ReadSpatialIntHexMap` |
| `float` | `ReadFloatHexMap` | `ReadSpatialFloatHexMap` |

These readers are available on both `HexMapFile` and `BinaryReader`. Use the reader matching the
written map kind and value type. Readers do not implicitly convert types or discard geometry.
Serialization of arbitrary `HexMap<T>` value types is not provided.

## Use a stream

```csharp
using System.IO;
using System.Text;
using Akeldov.Math.Hexes;

var mask = new BoolHexMap(new HexMapTopology(2, 1, Layout.OddR), new[] { true, false });
using var stream = new MemoryStream();
using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
writer.Write(mask);
writer.Flush();
stream.Position = 0;
using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
BoolHexMap restored = reader.ReadBoolHexMap(maxCellCount: 100);
```

The extensions leave the reader, writer, and stream open. The caller controls flushing and
disposal. Non-seekable streams are supported. Each successful read consumes exactly one record,
so several maps and other data can share a stream. Failed reads do not restore its position.
The source map must not change during serialization.

## Binary format and limits

Every record begins with a 16-byte header:

| Offset | Size | Field |
|---|---|---|
| 0 | 4 bytes | ASCII `HMAP`, without a length prefix |
| 4 | 1 byte | Format version `1`, independent of the package version |
| 5 | 1 byte | Map kind: `0` topology-only, `1` spatial |
| 6 | 1 byte | Value kind: `1` Boolean, `2` Int32, `3` Single |
| 7 | 4 bytes | Width as a non-negative little-endian Int32 |
| 11 | 4 bytes | Height as a non-negative little-endian Int32 |
| 15 | 1 byte | Layout: OddR=0, EvenR=1, OddQ=2, EvenQ=3 |

For spatial records, offsets 16, 20, and 24 store `Origin.X`, `Origin.Y`, and `Radius` as
little-endian IEEE 754 binary32 values. The origin is the zero hex's center; radius is measured
from center to vertex in coordinate-space units. The apothem is derived on reading.

Values follow at offset 16 or 28, in row-major order with X advancing first. Boolean cells use
one byte, exactly 0 or 1. Integer cells use four signed little-endian bytes. Floating-point cells
use four IEEE 754 binary32 bytes, preserving signed zero, subnormals, infinities, and NaN payloads.
Empty maps retain both dimensions and geometry. There is no compression, padding, or checksum.

`maxCellCount` defaults to `int.MaxValue`; pass an application-specific limit to reject
oversized allocations. Zero accepts only empty maps. Readers validate cell counts and geometry
before allocating values, and seekable streams are checked for the complete payload.

Invalid headers, unsupported kinds, invalid Boolean bytes, non-finite origins, non-positive or
non-finite radii, and excessive cell counts cause `InvalidDataException`. Incomplete records
cause `EndOfStreamException`. Null arguments cause `ArgumentNullException`, and negative
limits cause `ArgumentOutOfRangeException`. File-system exceptions propagate to the caller.
