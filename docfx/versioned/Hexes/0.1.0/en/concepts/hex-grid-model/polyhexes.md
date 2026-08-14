# Polyhexes

A polyhex is a finite set of occupied hex cells. Akeldov.Math.Hexes represents that set as a
rectangular Q/R mask: the first array dimension is Q, the second is R, and a `true` cell belongs
to the shape. <xref:Akeldov.Math.Hexes.Topology.Polyhex> is the immutable value used to retain and
share such a mask.

## The mask model

For a mask element `[q, r]`, the corresponding integer QRS coordinate is
`(q, r, -q - r)`. Both indices are zero-based and must satisfy:

```text
0 <= q < QRSResolution.Q
0 <= r < QRSResolution.R
```

`QRSResolution.Q` and `QRSResolution.R` are therefore array extents. The derived `S` component is
not a third array dimension. The rectangular Q/R range appears as a parallelogram when drawn on a
hex grid; no <xref:Akeldov.Math.Hexes.Layout> is involved in this representation.

The type deliberately stores a mask rather than enforcing the mathematical definition of a
connected polyhex. A mask may be empty, contain holes or false margins, or contain several
disconnected components. Its original dimensions are preserved.

## Choose a construction path

| Input | Occupied cells | Ownership |
| --- | --- | --- |
| `new Polyhex(bool[,])` | Elements equal to `true` | The array is copied. |
| `new Polyhex(int[,])` | All nonzero elements | The converted mask is copied. |
| `new Polyhex(VectorQRSInt)` | None | A new empty mask with the given Q and R extents is created. |
| Implicit conversion from `bool[,]` | Elements equal to `true` | Equivalent to the Boolean-mask constructor and copies the array. |
| <xref:Akeldov.Math.Hexes.Topology.PolyhexBuilder> | Cells selected through writable indexers | `ToPolyhex()` takes an independent snapshot. |

Use a constructor when the whole mask already exists. Use a builder when cells are discovered or
edited incrementally.

## Create an immutable polyhex

The outer groups in a rectangular C# array initializer select Q; values inside a group select R:

```csharp
using Akeldov.Math.Hexes.Topology;

bool[,] mask =
{
    { true,  true,  false }, // q = 0, r = 0..2
    { false, true,  true  }  // q = 1, r = 0..2
};

var polyhex = new Polyhex(mask);

bool containsQ1R2 = polyhex[1, 2]; // true
int occupiedCellCount = polyhex.HexCount; // 4

mask[1, 2] = false;
bool stillPresent = polyhex[1, 2]; // true: the constructor copied the mask
```

An `int[,]` mask follows the same Q/R ordering. Zero means absent; both positive and negative
values mean present.

## Build a mask incrementally

<xref:Akeldov.Math.Hexes.Topology.PolyhexBuilder> has the same Q/R indexing but exposes setters:

```csharp
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;

var builder = new PolyhexBuilder(qSize: 3, rSize: 2);
builder[0, 0] = true;
builder[new VectorQRSInt(1, 0)] = true;

Polyhex snapshot = builder.ToPolyhex();

builder[0, 0] = false;
bool snapshotIsUnchanged = snapshot[0, 0]; // true
```

The constructor `new PolyhexBuilder(existingPolyhex)` also copies the existing value. A builder
never owns or mutates a `Polyhex`, and every `ToPolyhex()` call returns a new immutable snapshot.

## Read and export cells

<xref:Akeldov.Math.Hexes.Topology.IPolyhex> is the common read-only contract implemented by
`Polyhex` and geometry-aware polyhex types. It exposes:

| Member | Meaning |
| --- | --- |
| `QRSResolution` | Q and R extents of the retained mask |
| `HexCount` | Number of occupied cells, not total mask capacity |
| `this[q, r]` | Occupancy at Q/R mask indices |
| `this[VectorQRSInt]` | The same lookup using the coordinate's Q and R components |
| `GetExtended()` | A new mask containing the shape and its one-cell neighborhood |
| `GetContour()` | A new mask containing absent cells adjacent to the shape |

`Polyhex.ToBoolArray()` returns a new mutable `bool[,]` owned by the caller. Changing that array
does not change the polyhex:

```csharp
bool[,] editableCopy = polyhex.ToBoolArray();
editableCopy[0, 0] = false;

bool originalIsUnchanged = polyhex[0, 0]; // true
```

The interface itself only promises read access; the concrete `Polyhex` type supplies immutability
and value equality.

## Extend a shape and extract its cell boundary

Both topological operations return a new `Polyhex` with Q and R extents increased by two. Source
and result mask coordinates correspond as `[q, r]` to `[q + 1, r + 1]`; `GetContour()` uses that
correspondence without copying occupied source cells into the result.

`GetExtended()` marks the shifted source cells and all six of their QRS neighbors. A single
occupied source cell therefore becomes a seven-cell shape. False cells that are not adjacent to
the source remain false.

`GetContour()` marks each previously absent cell that touches an occupied source cell in one of
the six directions. It does not include the occupied source cells themselves. For a single cell,
the result is the six-cell ring around it; for a shape with holes, adjacent cells inside a hole
can also be part of the result.

Because these methods change both the resolution and the index origin, do not combine their
result with coordinates from the source mask without applying the `[+1, +1]` index shift. For
world-space boundary curves and filled regions, use the APIs described in [Geometry](geometry.md)
instead of treating `GetContour()` as a geometric contour.

## Add physical geometry

<xref:Akeldov.Math.Hexes.Geometry.IPolyhexGeometry> extends `IPolyhex` with two dimensions in
coordinate-space units:

- `HexRadius` is the distance from a cell center to a vertex;
- `HexApothem` is the distance from a cell center to an edge.

<xref:Akeldov.Math.Hexes.Geometry.PolyhexGeometry> is the standard implementation. It delegates
all mask reads to an immutable `Polyhex`, and it derives `HexApothem` as
`HexRadius * sqrt(3) / 2`:

| Constructor | Mask and ownership behavior |
| --- | --- |
| `PolyhexGeometry(Polyhex, radius)` | Retains the existing immutable value; it does not copy its mask again. |
| `PolyhexGeometry(bool[,], radius)` | Creates a `Polyhex` and copies the Boolean mask. |
| `PolyhexGeometry(int[,], radius)` | Creates a `Polyhex`; every nonzero element is occupied and the mask is copied. |
| `PolyhexGeometry(VectorQRSInt, radius)` | Creates an empty mask with the given Q and R extents. It does not create a filled rectangle. |

The radius must be finite and greater than zero; otherwise construction throws
`ArgumentOutOfRangeException`. A `null` `Polyhex` throws `ArgumentNullException`, and mask and
resolution inputs retain the validation rules described above. `HexRadius`, `HexApothem`,
`QRSResolution`, `HexCount`, and both occupancy indexers are read-only.

```csharp
using Akeldov.Math.Hexes.Geometry;

var geometry = new PolyhexGeometry(polyhex, radius: 2f);

float radius = geometry.HexRadius;   // 2 coordinate-space units
float apothem = geometry.HexApothem; // sqrt(3) coordinate-space units
bool occupied = geometry[1, 2];
```

`GetExtended()` and `GetContour()` still return plain topology-level `Polyhex` values. They do not
carry the radius into a new geometry wrapper. Wrap a result explicitly when its physical size
must be retained:

```csharp
var extended = geometry.GetExtended();
var extendedGeometry = new PolyhexGeometry(extended, geometry.HexRadius);
```

## Convert the mask to spatial regions

The extension methods on
<xref:Akeldov.Math.Hexes.Geometry.Contours.HexMatrixExtensions> turn any valid
`IPolyhexGeometry` into new Spatial2D `ContourBasedRegion` results:

| Operation | Result |
| --- | --- |
| `ToRegion(layout)` | The exact filled union of occupied regular hexes, bounded by straight segments. Shared internal edges are omitted. |
| `ToApothemOffsetRegion(layout)` | A filled region whose boundary is offset outward from the source boundary by `HexApothem`; convex joins use arcs with the same radius. |

Import `Akeldov.Math.Hexes.Geometry.Contours` to make the extensions available:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;

var geometry = new PolyhexGeometry(polyhex, radius: 2f);

var exactRegion = geometry.ToRegion(Layout.OddR);
var offsetRegion = geometry.ToApothemOffsetRegion(Layout.OddR);
```

The overloads without a `layout` argument use `Layout.OddR`. The layout is not stored by
`PolyhexGeometry`: choose it consistently each time a mask is converted. R layouts produce
pointy-top cells, while Q layouts produce flat-top cells. In this Q/R-mask path, odd and even
variants of the same orientation have identical placement; parity only matters when converting
offset row-and-column indices.

There is no custom-origin parameter. Conversion uses the library's default zero-hex placement:

| Layout | Center of mask cell `[0, 0]` |
| --- | --- |
| `OddR` or `EvenR` | `(HexApothem, HexRadius)` |
| `OddQ` or `EvenQ` | `(HexRadius, HexApothem)` |

Other cell centers are placed from that point along the QRS basis. Consequently, the first hex's
bounding box starts at zero, rather than its center lying at the spatial origin. Account for this
fixed placement when composing the returned contours with other spatial geometry; the wrapper
does not retain an origin.

Holes and disconnected occupied components are valid. `ToRegion()` creates separate closed
contours for hole and component boundary chains and relies on the even-odd fill rule.
`ToApothemOffsetRegion()` processes the same chains, splits intersecting offset sections, and
returns closed output boundary contours; do not assume its contour count or contour ordering is
the same as the source when offsets meet. Neither conversion changes the geometry wrapper or its
underlying mask.

Conversion failures are explicit:

- a `null` `IPolyhexGeometry` produces `ArgumentNullException`;
- a layout value other than `OddR`, `EvenR`, `OddQ`, or `EvenQ` produces
  `ArgumentOutOfRangeException`;
- a mask with no occupied cells (`HexCount == 0` for `PolyhexGeometry`) produces
  `InvalidOperationException` because it has no boundary;
- `ToRegion()` also reports `InvalidOperationException` if boundary segments cannot form closed
  continuous chains;
- `ToApothemOffsetRegion()` reports `InvalidOperationException` if usable offset boundary
  sections or closed chains cannot be formed.

The `PolyhexGeometry` constructor prevents invalid physical dimensions. A custom
`IPolyhexGeometry` implementation must likewise provide a finite positive radius and apothem and
consistent resolution and indexer values.

## Equality and identity

Two `Polyhex` instances are equal when their Q/R resolutions and every mask value are equal.
`Equals`, `==`, `!=`, and `GetHashCode()` use that structural definition. False margins are part
of the value, so two masks with the same occupied pattern but different resolutions are not equal.

Immutability keeps the hash code stable, making `Polyhex` suitable as a dictionary key. `ToString()`
is a diagnostic Q-major rendering of zeroes and ones, not a persistence format.

`PolyhexGeometry` is a reference type and does not override equality. Two geometry wrappers use
reference identity even when their masks and radii contain equal values.

## Validation and edge cases

- Boolean and integer source masks cannot be `null`; the constructors throw
  `ArgumentNullException`.
- Q and R dimensions must both be greater than zero. Empty array dimensions, a non-positive
  `QRSResolution.Q` or `.R`, and non-positive builder sizes produce `ArgumentOutOfRangeException`.
- An all-false mask is valid and has `HexCount == 0`.
- Reading or writing outside the retained Q/R range, including with a negative index, throws
  `IndexOutOfRangeException`. There is no clamping or wrapping.
- Dimension multiplication is checked; an unrepresentable backing-array length produces
  `OverflowException`.
- Construction does not crop false margins, fill holes, or check connectivity.

Validate external coordinates before indexing when absence outside the mask is normal control
flow.

## Binary serialization

The matching binary extensions preserve a nullable polyhex, its resolution, and its Q-major cell
values:

```csharp
using System.IO;
using Akeldov.Math.Hexes.Topology;

using var stream = new MemoryStream();
using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
{
    writer.Write(polyhex);
}

stream.Position = 0;
using var reader = new BinaryReader(stream);
Polyhex? restored = reader.ReadPolyhexStamp();

bool roundTripSucceeded = restored == polyhex; // true
```

The writer accepts `null`, and the reader returns `null` for the corresponding absent-value flag.
A successful non-null read creates a new `Polyhex`. Use the matching reader and writer from the
same data-format contract; the stream does not carry an independent schema version.

## Related concepts

- [QRS coordinates](../fundamentals/coordinate-systems/qrs-coordinates.md) explain the coordinate
  invariant and six neighbor directions.
- [Topology](topology.md) places finite cell sets in the wider grid model.
- [Geometry](geometry.md) adds cell size and converts masks into spatial boundaries and regions.
- <xref:Akeldov.Math.Hexes.Topology.IPolyhexWithPriority> extends the read-only contract with an
  integer ordering priority for custom competing shapes; `Polyhex` itself does not implement it.
