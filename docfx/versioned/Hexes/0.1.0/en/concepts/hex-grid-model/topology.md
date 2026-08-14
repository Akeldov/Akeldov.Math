# Topology

<xref:Akeldov.Math.Hexes.HexMapTopology> describes the finite index domain of a rectangular hex
map. It combines the number of columns and rows with the <xref:Akeldov.Math.Hexes.Layout> that
interprets those offset indices. This is enough to decide which cells belong to the map and how
their hex-grid relationships are interpreted.

Topology does not store cell values, a world-space origin, or a hex size. Keeping those concerns
separate lets the same topology drive maps, adjacency tables, geometry, and rasterization.

## Create a topology

Construct a topology from separate dimensions or from a `VectorXYInt` resolution:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var fromDimensions = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var fromResolution = new HexMapTopology(
    resolution: new VectorXYInt(4, 3),
    layout: Layout.OddR);

bool same = fromDimensions == fromResolution; // true
```

The resulting value exposes its resolution, cell count, and layout:

| Member | Meaning |
|---|---|
| `Resolution.X` | Number of columns, also called the width |
| `Resolution.Y` | Number of rows, also called the height |
| `Count` | Total cell count, `Resolution.X * Resolution.Y` |
| `Layout` | Offset convention and hex orientation |

Both resolution components must be non-negative. The layout must be `OddR`, `EvenR`, `OddQ`, or
`EvenQ`. Construction throws `ArgumentOutOfRangeException` for another value and
`OverflowException` when the cell count does not fit in `Int32`.

Zero is a valid dimension. A `0 x N`, `N x 0`, or `0 x 0` topology is empty and has `Count == 0`.
Some operations that need an actual spatial extent, such as calculating a whole-map bounding
box, reject an empty topology even though the topology value itself is valid.

## Understand the rectangular domain

The rectangle is expressed in row-and-column offset indices. For a topology with width `W` and
height `H`, an index belongs to the map exactly when:

```text
0 <= X < W
0 <= Y < H
```

The upper bounds are exclusive. In particular, `(W, 0)` and `(0, H)` are outside the map.
`HexMapTopology` is a descriptor and does not expose an indexer or a containment method, so check
these bounds before passing a computed index to a map:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var index = new VectorXYInt(2, 1);

bool contains =
    index.X >= 0 && index.X < topology.Resolution.X &&
    index.Y >= 0 && index.Y < topology.Resolution.Y;
```

Coordinate conversion and adjacency helpers operate on the infinite hex grid. They can therefore
produce a negative or otherwise out-of-bounds index; the topology does not clip or wrap it.

## Keep layout with the dimensions

A `W x H` rectangle can be interpreted under four offset-coordinate layouts. `OddR` and `EvenR`
use pointy-top hexes and stagger rows; `OddQ` and `EvenQ` use flat-top hexes and stagger columns.
Changing only the layout leaves the index count and rectangular bounds unchanged, but it changes
QRS conversion, adjacency, and spatial placement.

For example, the storage index `(1, 1)` is a valid cell in both of these topologies, but its QRS
coordinate and neighbors are interpreted through a different layout:

```csharp
var oddRows = new HexMapTopology(4, 3, Layout.OddR);
var evenRows = new HexMapTopology(4, 3, Layout.EvenR);

bool equal = oddRows == evenRows; // false
```

Treat the layout as part of the map's identity rather than as a rendering option. See
[Layouts](../fundamentals/layouts.md) for the four offset conventions and
[Row and Column Indices](../fundamentals/coordinate-systems/row-and-column-indices.md) for exact
QRS conversions.

## Map indices to linear storage

Topology-backed maps use row-major order. `X` advances first, so coordinates `(x, y)` map to:

```text
flatIndex = y * width + x
```

The reverse mapping for a non-empty width is:

```text
x = flatIndex % width
y = flatIndex / width
```

For a valid cell, `flatIndex` is in the range `0` through `Count - 1`. The layout does not affect
this mapping; it affects the hex meaning of the `(x, y)` index.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var terrain = new HexMap<string>(topology);
var index = new VectorXYInt(2, 1);

terrain[index] = "forest";

int flatIndex = index.Y * topology.Resolution.X + index.X; // 6
string value = terrain[flatIndex];                         // "forest"
```

`HexMap<T>` validates a `VectorXYInt` against both dimensions. Its flat integer indexer follows
the normal array bounds rules.

## Use immutable value semantics

`HexMapTopology` is a read-only value type. It can be copied and shared without ownership concerns,
and equality compares both `Resolution` and `Layout`. It also supports deconstruction:

```csharp
var topology = new HexMapTopology(4, 3, Layout.OddQ);

var (width, height, layout) = topology;

// width == 4, height == 3, layout == Layout.OddQ
string text = topology.ToString();
// HexMapTopology(width: 4, height: 3, layout: OddQ)
```

This makes a topology suitable as a configuration value or dictionary key. Two topologies with
the same dimensions but different layouts are deliberately unequal.

## Separate topology, geometry, and data

Choose the smallest model that contains the information an operation needs:

| Model | Contains | Use it for |
|---|---|---|
| `HexMapTopology` | Resolution and layout | Index bounds, cell count, index relationships |
| `HexMapGeometry` | Topology, world-space origin, and hex radius | Centers, vertices, map bounds, raster placement |
| `HexMap<T>` | Topology and one mutable value per cell | Terrain, costs, masks, and other cell data |

The same topology can be passed to several maps so their indices describe the same cells. When
combining maps, compare their complete topology values, not only their resolutions.

Use <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> when an operation needs world-space coordinates;
continue to [Geometry](geometry.md) for that layer. See [Maps](../data-storage/maps.md) for
topology-backed value storage and
[Complete and Partial Neighborhoods](../data-storage/complete-and-partial-neighborhoods.md) for
precomputed adjacency at map boundaries.
