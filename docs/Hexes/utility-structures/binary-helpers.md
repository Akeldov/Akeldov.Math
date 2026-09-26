# Binary Helpers

Binary helpers provide shared serialization support for hex-grid utility values.

## Readers

- Read QRS vectors.
- Read integer QRS vectors.
- Read sixfold angles.
- Read polyhex stamps.

## Writers

- Write QRS vectors.
- Write integer QRS vectors.
- Write sixfold angles.
- Write polyhex stamps.

## Validation

- Null readers and writers are rejected.
- Invalid serialized enum values are rejected.

## Boolean maps

Version 0.6.0 adds `BinaryWriter.Write(IHexMap<bool>)` and
`BinaryReader.ReadBoolHexMap(maxCellCount)`. Import `Akeldov.Math.Hexes` to use these extensions.
They preserve the map's width, height, layout, and Boolean values. Reading returns a new mutable
`BoolHexMap` with independent storage.

```csharp
using System.IO;
using Akeldov.Math.Hexes;

var map = new BoolHexMap(
    new HexMapTopology(3, 2, Layout.EvenQ),
    new[] { true, false, false, true, true, false });

using (var writer = new BinaryWriter(File.Create("mask.hmap")))
{
    writer.Write(map);
}

BoolHexMap restored;
using (var reader = new BinaryReader(File.OpenRead("mask.hmap")))
{
    restored = reader.ReadBoolHexMap(maxCellCount: 1_000_000);
}
```

The writer accepts any `IHexMap<bool>`, including `BoolHexMap` and `HexMap<bool>`. This operation
saves topology and values only: if a spatial map is supplied through the interface, its origin
and radius are not included. The source must not change during serialization.

The extensions operate at the current stream position and leave the reader, writer, and stream
open. They support streams without seeking. Each successful read consumes exactly one record,
so multiple maps and other application data can share a stream. The caller controls flushing and
disposal; a failed read does not restore the stream position.

### Version 1 format

The format version is independent of the NuGet package version. The header is exactly 16 bytes:

| Offset | Size | Field |
|---|---|---|
| 0 | 4 bytes | ASCII signature `HMAP`, without a string-length prefix |
| 4 | 1 byte | Format version: `1` |
| 5 | 1 byte | Map kind: `0` for topology and values |
| 6 | 1 byte | Value kind: `1` for Boolean values |
| 7 | 4 bytes | Width, a non-negative little-endian `Int32` |
| 11 | 4 bytes | Height, a non-negative little-endian `Int32` |
| 15 | 1 byte | Layout: `0` = `OddR`, `1` = `EvenR`, `2` = `OddQ`, `3` = `EvenQ` |

The header is followed by `width * height` bytes, each exactly `0` for false or `1` for true.
Values are stored in row-major order: X advances first, and `(x, y)` maps to `y * width + x`.
There is no padding or trailing marker. Empty maps have no payload and preserve their original
dimensions, including `0 x N` and `N x 0`.

### Reading limits and errors

`maxCellCount` defaults to `Int32.MaxValue`. Pass an application-specific limit to bound the
value-array allocation; zero accepts only empty maps. The reader validates the header and cell
count before allocating storage. For seekable streams, it also checks that the remaining bytes
can hold the declared payload before allocating.

- Null readers, writers, and maps cause `ArgumentNullException`.
- A negative `maxCellCount` causes `ArgumentOutOfRangeException`.
- Invalid signatures, versions, kinds, dimensions, layouts, Boolean bytes, and excessive cell
  counts cause `InvalidDataException`.
- Incomplete records cause `EndOfStreamException`.

The format does not include a checksum: a changed byte that is still a valid field or Boolean
value cannot be distinguished from intentional data.

## Integer maps

Version 0.6.0 adds `BinaryWriter.Write(IHexMap<int>)` and
`BinaryReader.ReadIntHexMap(maxCellCount)`. They preserve the topology and every signed `Int32`
value, including `Int32.MinValue`, `Int32.MaxValue`, zero, and negative values. Reading returns a
new mutable `IntHexMap` with independent storage.

```csharp
using System.IO;
using Akeldov.Math.Hexes;

var map = new IntHexMap(
    new HexMapTopology(3, 2, Layout.EvenQ),
    new[] { int.MinValue, int.MaxValue, 0, -1, 0x12345678, -2 });

using (var writer = new BinaryWriter(File.Create("costs.hmap")))
{
    writer.Write(map);
}

IntHexMap restored;
using (var reader = new BinaryReader(File.OpenRead("costs.hmap")))
{
    restored = reader.ReadIntHexMap(maxCellCount: 1_000_000);
}
```

The writer accepts any `IHexMap<int>`, including `IntHexMap` and `HexMap<int>`. A spatial source
contributes its topology and values; its origin and radius are not serialized. The source must
not change during serialization.

Integer records use the same [version-1 header](#version-1-format), with value kind **`2`** at
offset 6. The payload contains `width * height` signed little-endian `Int32` values, each encoded
as four bytes in two's-complement representation, in row-major order. The total record size is
`16 + 4 * width * height` bytes. The existing Boolean record format remains unchanged.

The same stream lifetime rules and [reading limits and errors](#reading-limits-and-errors)
apply. `maxCellCount` limits the number of cells, not bytes: each integer cell requires four
payload bytes. The seekable-stream payload-length check uses 64-bit arithmetic before allocating
the value array. Truncation within a four-byte integer also causes `EndOfStreamException`.

Boolean and integer records can share a stream when read in the written order with their
respective methods. `ReadIntHexMap` rejects Boolean records, and `ReadBoolHexMap` rejects integer
records with `InvalidDataException`; neither reader converts another value kind implicitly.

## Floating-point maps

Version 0.6.0 adds `BinaryWriter.Write(IHexMap<float>)` and
`BinaryReader.ReadFloatHexMap(maxCellCount)`. They preserve the topology and the raw bits of every
`Single` value. Reading returns a new mutable `FloatHexMap` with independent storage.

```csharp
using System.IO;
using Akeldov.Math.Hexes;

var map = new FloatHexMap(
    new HexMapTopology(3, 2, Layout.EvenQ),
    new[] { 1.5f, -2.25f, 0f, -0f, float.PositiveInfinity, float.NaN });

using (var writer = new BinaryWriter(File.Create("heights.hmap")))
{
    writer.Write(map);
}

FloatHexMap restored;
using (var reader = new BinaryReader(File.OpenRead("heights.hmap")))
{
    restored = reader.ReadFloatHexMap(maxCellCount: 1_000_000);
}
```

The writer accepts any `IHexMap<float>`, including `FloatHexMap` and `HexMap<float>`. A spatial
source contributes its topology and values; its origin and radius are not serialized. The source
must not change during serialization.

Floating-point records use the same [version-1 header](#version-1-format), with value kind **`3`**
at offset 6. The payload contains `width * height` IEEE 754 binary32 values, each stored as four
little-endian bytes, in row-major order. The total record size is `16 + 4 * width * height` bytes.
The Boolean and integer record formats remain unchanged.

Serialization performs no rounding, normalization, or arithmetic conversion. Both signs of zero,
subnormal values, finite extrema, positive and negative infinity, and NaN signs and payloads are
preserved. All 32-bit value patterns are accepted. Use `BitConverter.SingleToInt32Bits` when
comparing restored bits; floating-point equality cannot distinguish signed zeros or NaN payloads.

The same stream lifetime rules and [reading limits and errors](#reading-limits-and-errors) apply.
`maxCellCount` limits cells, not bytes; each cell requires four payload bytes. Seekable streams
are checked for the full payload using 64-bit arithmetic before allocation. A record truncated
within a value causes `EndOfStreamException`.

Boolean, integer, and floating-point records can share a stream. Read them in the written order
using `ReadBoolHexMap`, `ReadIntHexMap`, and `ReadFloatHexMap`, respectively. A reader for another
value kind rejects the record with `InvalidDataException`, even when both types use four-byte cells.

## Spatial maps

Use `BinaryWriter.Write` with `ISpatialHexMap<bool>`, `ISpatialHexMap<int>`, or
`ISpatialHexMap<float>` to preserve topology, origin, radius, and cell values. These overloads
accept both the specialized spatial maps and generic `SpatialHexMap<T>` sources. Read them with
`ReadSpatialBoolHexMap`, `ReadSpatialIntHexMap`, and `ReadSpatialFloatHexMap`, respectively.
Each reader returns a new mutable specialized spatial map with independent storage.

```csharp
using System.IO;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    new HexMapTopology(3, 2, Layout.EvenQ),
    new VectorXY(-10f, 20f),
    radius: 2f);
var map = new SpatialFloatHexMap(
    geometry, new[] { 1.5f, -2.25f, 0f, -0f, float.PositiveInfinity, float.NaN });

using (var writer = new BinaryWriter(File.Create("terrain.hmap")))
{
    writer.Write(map);
}

SpatialFloatHexMap restored;
using (var reader = new BinaryReader(File.OpenRead("terrain.hmap")))
{
    restored = reader.ReadSpatialFloatHexMap(maxCellCount: 1_000_000);
}
```

`Write` selects the format using the compile-time type of its argument. Concrete spatial maps
and `ISpatialHexMap<T>` sources select the overload that preserves geometry. Sources typed as
`IHexMap<T>` select the topology-only overload, even if the underlying map is spatial.
Choose the corresponding reader; topology-only readers reject spatial records and spatial readers
reject topology-only records. Readers also reject other value kinds without converting them.

### Spatial version 1 format

Spatial records use the same [16-byte header](#version-1-format), with map kind **`1`** at
offset 5. Value kind remains `1` for Boolean, `2` for Int32, or `3` for Single. Geometry follows:

| Offset | Size | Field |
|---|---|---|
| 16 | 4 bytes | Origin.X, little-endian IEEE 754 binary32 |
| 20 | 4 bytes | Origin.Y, little-endian IEEE 754 binary32 |
| 24 | 4 bytes | Radius, little-endian IEEE 754 binary32 |
| 28 | Variable | Row-major cell values, X advancing first |

Origin is the center of the zero hex. Radius is the distance from a hex center to a vertex,
in coordinate-space units. The apothem is derived from the radius when reading.
Geometry fields retain their exact bits, including negative zero in origin coordinates.
Cell encoding is identical to the matching topology-only format: one canonical Boolean byte,
four Int32 bytes, or four raw Single bytes. All floating-point cell bit patterns are supported.
The total size is `28 + width * height * bytesPerCell`. Empty spatial maps occupy 28 bytes
and retain both dimensions and all geometry fields.

### Spatial validation and streams

The same [reading limits and errors](#reading-limits-and-errors) and stream lifetime rules apply.
Readers additionally reject non-finite origin components and non-positive or non-finite radii
with `InvalidDataException`. Geometry and cell limits are validated before value storage is
allocated. Seekable streams are checked for the full payload with 64-bit arithmetic.
Truncation in the header, geometry, or values causes `EndOfStreamException`.

Writers reject invalid geometry with `ArgumentOutOfRangeException`, and reject a custom
source whose `Geometry.Topology` differs from `Topology` with `ArgumentException`,
before writing any bytes. Sources must not change during serialization.
All six map variants can share a stream when read in the written order with the matching methods.

## Map files

`HexMapFile` opens and closes files for all six map variants, using the same binary serializers:

```csharp
using Akeldov.Math.Hexes;

HexMapFile.Write("mask.hmap", boolMap);
HexMapFile.Write("costs.hmap", intMap);
HexMapFile.Write("heights.hmap", floatMap);
HexMapFile.Write("spatial-mask.hmap", spatialBoolMap);
HexMapFile.Write("spatial-costs.hmap", spatialIntMap);
HexMapFile.Write("spatial-heights.hmap", spatialFloatMap);

BoolHexMap mask = HexMapFile.ReadBoolHexMap("mask.hmap");
IntHexMap costs = HexMapFile.ReadIntHexMap("costs.hmap");
FloatHexMap heights = HexMapFile.ReadFloatHexMap("heights.hmap");
SpatialBoolHexMap spatialMask = HexMapFile.ReadSpatialBoolHexMap("spatial-mask.hmap");
SpatialIntHexMap spatialCosts = HexMapFile.ReadSpatialIntHexMap("spatial-costs.hmap");
SpatialFloatHexMap spatialHeights = HexMapFile.ReadSpatialFloatHexMap(
    "spatial-heights.hmap", maxCellCount: 1_000_000);
```

`Write` has overloads for `IHexMap<bool/int/float>` and `ISpatialHexMap<bool/int/float>`.
A concrete spatial type or an `ISpatialHexMap<T>` variable selects the overload that preserves
origin and radius. If a spatial source is passed through an `IHexMap<T>` variable, the selected
overload writes topology and values only. Overload selection uses the compile-time type.

Paths may be absolute or relative. Relative paths are resolved against the process's current
working directory, which may differ from the executable's directory. Parent directories must
already exist. Writing creates a file or truncates an existing one; it does not append.
Writes are not atomic, so an error after opening the file can leave a partial record.
Null maps and invalid spatial geometry are rejected before opening the destination file.

Each reader returns a new mutable specialized map, reads the first record, and ignores any
trailing data. Every reader supports the optional `maxCellCount` limit with the same meaning
and default as the stream methods. Files are closed on both success and failure; successful
writes are flushed before returning. Serialization and file-system exceptions propagate
to the caller. Use the stream extensions when reading or writing multiple records in one file.
