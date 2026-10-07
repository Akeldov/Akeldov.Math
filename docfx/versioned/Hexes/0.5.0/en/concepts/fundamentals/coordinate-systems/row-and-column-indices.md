# Row and Column Indices

Akeldov.Math.Hexes uses `VectorXYInt` as the rectangular storage index for a hex map. `X` is the
column and `Y` is the row. This makes maps compatible with ordinary two-dimensional arrays and
row-major storage, while <xref:Akeldov.Math.Hexes.Layout> records how the hexes in alternating
rows or columns are offset.

An XY index is a storage address, not a Cartesian position and not a layout-independent hex
coordinate.

## Interpret the index through its layout

The four layouts use the same `VectorXYInt` type:

| Layout | Hex orientation | Staggered dimension |
|---|---|---|
| `OddR` | Pointy-top | Odd rows |
| `EvenR` | Pointy-top | Even rows |
| `OddQ` | Flat-top | Odd columns |
| `EvenQ` | Flat-top | Even columns |

The numeric pair `(2, 3)` therefore does not identify a complete hex-grid address by itself. Keep
the associated layout with the map or pass it to every conversion and adjacency operation.

See [Layouts](../layouts.md) for the orientation and offset conventions.

## Convert QRS to a storage index

Use `ToXYIndex(Layout)` to convert an integer QRS coordinate. Use
`ToQRSIndex(Layout)` for the exact inverse:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
var qrs = new VectorQRSInt(q: 2, r: -1);

VectorXYInt index = qrs.ToXYIndex(layout);
VectorQRSInt restored = index.ToQRSIndex(layout);

// index is (1, -1)
// restored equals qrs
```

The round trip is exact when both calls use the same layout. Converting back with another layout
usually produces a different QRS coordinate.

For reference, let `p(n) = n & 1`, which is `0` for even integers and `1` for odd integers,
including negative odd integers. QRS-to-XY conversion uses these formulas:

| Layout | Column `X` | Row `Y` |
|---|---|---|
| `OddR` | `Q + (R - p(R)) / 2` | `R` |
| `EvenR` | `Q + (R + p(R)) / 2` | `R` |
| `OddQ` | `Q` | `R + (Q - p(Q)) / 2` |
| `EvenQ` | `Q` | `R + (Q + p(Q)) / 2` |

The numerators are even, so integer division is exact. Use the library methods instead of
repeating these formulas in application code; the table is intended to explain the mapping.

## Address rectangular maps

<xref:Akeldov.Math.Hexes.HexMapTopology> combines a non-negative `VectorXYInt` resolution with a
layout. For a topology with width `W` and height `H`, valid indices satisfy:

```text
0 <= X < W
0 <= Y < H
```

`HexMap<T>` stores one value per valid index in row-major order. `X` advances first, and the flat
index is `Y * W + X`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);
var terrain = new HexMap<string>(topology);

var index = new VectorXYInt(1, 2);
terrain[index] = "forest";

int flatIndex = index.Y * topology.Resolution.X + index.X; // 9
```

The `VectorXYInt` indexer checks both map bounds. A QRS/XY conversion operates on the infinite
coordinate grid and does not clip its result to a particular map, so check the result before
using it as a map index.

## Find adjacent indices

Topology extensions calculate neighbors directly in offset coordinates while applying the
layout's parity rules:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
var center = new VectorXYInt(1, 1);

VectorXYInt next = center.GetAdjacent(
    SixfoldAngle.Deg60,
    layout);
VectorXYInt[] neighbors = center.GetAdjacents(layout);
```

`GetAdjacents` returns a new array containing all six edge-adjacent indices. Like coordinate
conversion, adjacency is calculated on the infinite grid and can return negative or out-of-map
indices.

Do not infer hex distance or all neighbor directions from ordinary `X`/`Y` subtraction. The
stagger changes with row or column parity. Use the adjacency helpers, or convert both indices to
QRS before layout-independent grid arithmetic.

## Work with negative indices

Coordinate conversion and adjacency support negative rows and columns. Parity is numeric:
`-1` and `-3` are odd, while `-2` is even. The conversion formulas deliberately use bit parity
so that negative values round-trip correctly.

Negative indices are useful for an unbounded logical grid, temporary offsets, and maps whose
world origin is not at storage index zero. They remain outside a rectangular `HexMap<T>` unless
application code translates or clips them into its valid range.

Continue to [Spatial Coordinates](spatial-coordinates.md) to place an index in world space, or
see [Coordinate Discretization](../coordinate-discretization.md) to obtain an index from a point.
