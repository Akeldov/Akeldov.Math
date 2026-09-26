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

## Boolean maps (upcoming)

The upcoming release adds `BinaryWriter.WriteHexMap(IHexMap<bool>)` and
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
    writer.WriteHexMap(map);
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

## Integer maps (upcoming)

The upcoming release also adds `BinaryWriter.WriteHexMap(IHexMap<int>)` and
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
    writer.WriteHexMap(map);
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
