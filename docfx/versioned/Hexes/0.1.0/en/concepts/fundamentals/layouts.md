# Layouts

A <xref:Akeldov.Math.Hexes.Layout> describes how a rectangular `VectorXYInt` index is placed on
the hex lattice. It combines two choices: the orientation of each hexagon and which alternating
rows or columns receive the offset.

The layout is therefore more specific than <xref:Akeldov.Math.Hexes.HexOrientation>. Orientation
distinguishes pointy-top from flat-top hexagons, but it cannot distinguish `OddR` from `EvenR` or
`OddQ` from `EvenQ`.

## The four layouts

| Layout | Orientation | Staggered dimension | Offset parity |
| --- | --- | --- | --- |
| `OddR` | `PointyTop` | rows (`Y`, or QRS `R`) | odd rows |
| `EvenR` | `PointyTop` | rows (`Y`, or QRS `R`) | even rows |
| `OddQ` | `FlatTop` | columns (`X`, or QRS `Q`) | odd columns |
| `EvenQ` | `FlatTop` | columns (`X`, or QRS `Q`) | even columns |

In a pointy-top layout, a hex has a vertex at the top and rectangular storage is row-oriented.
In a flat-top layout, a hex has a horizontal edge at the top and storage is column-oriented.
The `R` and `Q` suffixes identify the QRS component that is preserved as the row or column index.

With the Spatial2D convention of positive X to the right and positive Y upward, the selected rows
are displaced along positive X and the selected columns along positive Y. Diagrams in which row
indices increase downward show the column displacement in the opposite visual direction; the
odd/even convention and conversion formulas do not change.

Use the orientation helpers when code only needs the geometric distinction:

```csharp
using Akeldov.Math.Hexes;

Layout layout = Layout.OddR;

bool isPointy = layout.IsPointyTop();       // true
bool isFlat = layout.IsFlatTop();           // false
HexOrientation orientation = layout.GetHexOrientation(); // PointyTop
```

`GetHexOrientation()` rejects an unsupported enum value. The two predicates classify the two
defined layout pairs and return `false` for values outside their pair.

## Offset indices and QRS indices

`VectorXYInt` is convenient for a rectangular array: `X` selects a column and `Y` selects a row.
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> identifies the same cell independently of the
odd/even offset convention. Convert between them with `ToQRSIndex(layout)` and
`ToXYIndex(layout)`.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
var storageIndex = new VectorXYInt(-3, -3);

VectorQRSInt hexIndex = storageIndex.ToQRSIndex(layout);
// Q = -1, R = -3, S = 4

VectorXYInt restored = hexIndex.ToXYIndex(layout);
// X = -3, Y = -3
```

For every supported layout, converting an integer index in both directions with the same layout
restores the original value when the converted components fit in `Int32`. This is an integer
re-encoding, not nearest-hex rounding. The conversions preserve mathematical odd/even parity for
negative indices as well as positive ones.

See [Row and Column Indices](coordinate-systems/row-and-column-indices.md) for the exact conversion
formulas, rectangular storage rules, and the parity caveat for negative indices.

## Keep one layout through an operation

A `VectorXYInt` does not carry its layout. Interpreting it with a different layout can silently
select a different logical hex. Likewise, converting a QRS index back with another layout can
change its storage index:

```csharp
var storageIndex = new VectorXYInt(-3, -3);
VectorQRSInt hexIndex = storageIndex.ToQRSIndex(Layout.OddR);

VectorXYInt sameCell = hexIndex.ToXYIndex(Layout.OddR);  // (-3, -3)
VectorXYInt otherEncoding = hexIndex.ToXYIndex(Layout.EvenR); // (-2, -3)
```

Choose the layout once and pass that value to topology, geometry, coordinate conversion,
neighborhood, and rasterization APIs. <xref:Akeldov.Math.Hexes.HexMapTopology> stores the choice in
its `Layout` property; geometry built from that topology keeps the same value.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

const Layout layout = Layout.OddR;
var topology = new HexMapTopology(width: 32, height: 24, layout: layout);
var geometry = new HexMapGeometry(topology, origin: VectorXY.Zero, radius: 10f);
```

Do not persist only `HexOrientation` when an offset index must be reconstructed: it loses the
odd/even convention. Persist the complete `Layout` with the indexed data.

## Layouts in continuous space

The odd and even variants of one orientation share the same continuous QRS basis. Consequently,
continuous QRS-to-spatial conversions treat `OddR` and `EvenR` alike, and treat `OddQ` and
`EvenQ` alike. The odd/even choice becomes relevant when the result is encoded as a rectangular
`VectorXYInt`.

For example, `PointXY.ToXYIndex(hexRadius, hexFieldOrigin, layout)` first locates the nearest QRS
cell using the layout orientation and then encodes it using the selected odd/even rule. Pass the
same radius, origin, and layout that define the map geometry.

Read [Coordinate Systems](coordinate-systems/index.md) for the roles of QRS, offset, and spatial
coordinates, then continue with [Coordinate Discretization](coordinate-discretization.md) for
world-space point conversion and nearest-hex rounding.
