# Hex Grid Geometry

Hex-grid geometry places logical hex indices in the continuous Cartesian space provided by
Akeldov.Math.Spatial2D. A <xref:Akeldov.Math.Hexes.HexMapTopology> says which cells exist and how
they are indexed; <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> adds their physical size and
position.

All distances and positions use one application-defined coordinate-space unit. It may represent
pixels, metres, or game-world units, but a single operation must use it consistently.

## Size and orientation

A regular hex is described by two distances:

- **radius** `R`: center to vertex;
- **apothem** `A`: center to the midpoint of an edge.

They are related by:

```text
A = R * sqrt(3) / 2
R = A * 2 / sqrt(3)
```

`ConvertHexRadiusToApothem()` and `ConvertHexApothemToRadius()` perform these conversions. Both
values must be finite and greater than zero.

The <xref:Akeldov.Math.Hexes.Layout> determines the orientation as well as the odd/even offset
rule:

| Layouts | Orientation | First normalized vertex | Vertex angles |
| --- | --- | --- | --- |
| `OddR`, `EvenR` | `PointyTop` | 30 degrees | 30, 90, 150, 210, 270, 330 degrees |
| `OddQ`, `EvenQ` | `FlatTop` | 0 degrees | 0, 60, 120, 180, 240, 300 degrees |

Angles in the table are measured counterclockwise from positive X in Spatial2D coordinates.
Odd and even variants have the same hex shape and QRS basis; they differ only when QRS cells are
encoded as rectangular `VectorXYInt` indices. See [Layouts](../fundamentals/layouts.md) for the
four offset conventions.

## Describe map placement

`HexMapGeometry` is an immutable value that keeps four related settings together:

- `Topology`: resolution and layout;
- `Origin`: the `VectorXY` center of the zero hex, whose QRS and storage indices are both zero;
- `Radius`: center-to-vertex distance in coordinate-space units;
- `Apothem`: the derived center-to-edge distance in the same units.

Use an explicit origin when a grid must align with an existing coordinate system:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 8,
    height: 6,
    layout: Layout.OddR);

var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 10f);
```

The overload without `origin` uses a layout-dependent default:

| Orientation | Default zero-hex center |
| --- | --- |
| `PointyTop` | `(apothem, radius)` |
| `FlatTop` | `(radius, apothem)` |

This default places a single zero hex against positive coordinate axes. It is not the minimum
corner of the whole map. In particular, shifted rows in `EvenR` and shifted columns in `EvenQ`
can extend below the corresponding default X or Y coordinate.

## Calculate hex centers

For a storage index `(x, y)`, an origin `(originX, originY)`, radius `R`, and apothem `A`, the
center formulas are:

| Layout | Center X | Center Y |
| --- | --- | --- |
| `OddR` | `originX + 2A*x + A*(y & 1)` | `originY + 1.5R*y` |
| `EvenR` | `originX + 2A*x - A*(y & 1)` | `originY + 1.5R*y` |
| `OddQ` | `originX + 1.5R*x` | `originY + 2A*y + A*(x & 1)` |
| `EvenQ` | `originX + 1.5R*x` | `originY + 2A*y - A*(x & 1)` |

The bitwise parity expression also identifies negative odd indices correctly. Prefer
`VectorXYInt.GetHexCenter()` to duplicating these formulas:

```csharp
var index = new VectorXYInt(2, 3);

VectorXY center = index.GetHexCenter(
    geometry.Radius,
    geometry.Origin,
    geometry.Topology.Layout);
```

The overload without an origin uses the default center shown above. For a QRS index, call
`VectorQRSInt.GetHexOffset(radius, layout)` and add the desired origin. The static
`GetHexCenter(q, r, radius, layout)` helper also accepts Q and R directly, but uses the default
origin.

The same radius and layout must be used throughout a conversion. For the QRS-to-XY basis and
inverse conversion, see [Spatial Coordinates](../fundamentals/coordinate-systems/spatial-coordinates.md).

## Generate vertices

`GetHexVertices(radius, layout)` expands a known `VectorXY` center into six vertices:

```csharp
VectorXY[] vertices = center.GetHexVertices(
    geometry.Radius,
    geometry.Topology.Layout);
```

The result is a new mutable six-element array owned by the caller. Vertices follow the
counterclockwise order in the orientation table. `GetNormalizedHexVertices(layout)` returns the
same offsets for a unit-radius hex, also as a new caller-owned array; multiply each offset by the
radius and add the center to obtain world positions.

There is also a QRS overload, `GetHexVertices(q, r, radius, layout)`. Like the corresponding
center helper, it uses the layout's default zero-hex center.

To identify a nearby vertex, use:

- `PointXY.GetClosestVertexIndex(radius, center, layout)` for a known hex; it returns an index
  from 0 through 5 in the same vertex order;
- `PointXY.GetClosestHexVertexIndex(radius, origin, layout)` to classify the containing hex first
  and return its `VectorXYInt` index together with a `HexVertex` value.

On a shared hex boundary, the containing cell follows the same tie behavior as coordinate
discretization. If two vertices are exactly equidistant, the lower vertex index wins because the
vertices are tested in order. See [Coordinate Discretization](../fundamentals/coordinate-discretization.md)
for point-to-cell boundary behavior.

## Bound a rectangular map

`HexMapGeometry.GetBoundingBox()` returns the axis-aligned `Rectangle` containing every complete
hex in the map. `GetBoundingBoxSize()` returns only its `VectorXY` size. The bounds include the
outer radius or apothem around the first and last centers; they are not just the extent of the
center points.

For positive width `W` and height `H`, the size is:

| Orientation | Bounding width | Bounding height |
| --- | --- | --- |
| `PointyTop` | `2A*W + (H == 1 ? 0 : A)` | `2R + 1.5R*(H - 1)` |
| `FlatTop` | `2R + 1.5R*(W - 1)` | `2A*H + (W == 1 ? 0 : A)` |

The odd/even variant does not change the size, but it can change the minimum corner because its
shift points in the opposite direction. Let `O = (originX, originY)`:

| Layout | Minimum X | Minimum Y |
| --- | --- | --- |
| `OddR` | `originX - A` | `originY - R` |
| `EvenR` | `originX - A*(H == 1 ? 1 : 2)` | `originY - R` |
| `OddQ` | `originX - R` | `originY - A` |
| `EvenQ` | `originX - R` | `originY - A*(W == 1 ? 1 : 2)` |

Use the API for production calculations so the size and minimum corner stay consistent:

```csharp
var bounds = geometry.GetBoundingBox();
VectorXY size = geometry.GetBoundingBoxSize();

// Equivalent when geometry is not otherwise needed:
var sameBounds = topology.GetBoundingBox(
    geometry.Origin,
    geometry.Radius);
```

A topology may have a zero width or height, but an empty map has no geometric bounding
rectangle. Both bounding methods therefore require `W > 0` and `H > 0` and throw
`ArgumentOutOfRangeException` for an empty map.

## Precompute all centers

<xref:Akeldov.Math.Hexes.Geometry.HexCenterMap> precomputes one `PointXY` center for each map cell.
It is useful when many operations repeatedly sample the same geometry:

```csharp
var centers = new HexCenterMap(geometry);

PointXY cellCenter = centers[new VectorXYInt(2, 3)];
PointXY sameCell = centers[3 * topology.Resolution.X + 2];
```

The map is read-only and exposes the geometry and topology used to build it. Both indexers follow
row-major storage; the `VectorXYInt` indexer checks X and Y against the topology. Constructing a
`HexCenterMap` from only a topology uses unit radius and the default origin, so pass an explicit
`HexMapGeometry` when physical scale or placement matters.

## Geometry invariants

Keep these contracts at API boundaries:

- radii and apothems must be finite and greater than zero;
- explicit origins, centers, and classified points must contain finite components;
- a layout must be one of `OddR`, `EvenR`, `OddQ`, or `EvenQ`;
- the origin is a center, not a bounding-box corner;
- `VectorXY` and `PointXY` do not carry units, radius, origin, or layout, so those settings must
  travel with the values that depend on them;
- arrays returned by vertex helpers are new and caller-owned.

Continue with [Topology](topology.md) for cell membership and adjacency, or
[Polyhexes](polyhexes.md) for geometry built from finite cell masks. For focused code,
follow [Get a Hex Center and Vertices](../../how-to-guides/geometry-and-polyhexes/get-a-hex-center-and-vertices.md)
or [Get Map Geometry Bounds](../../how-to-guides/geometry-and-polyhexes/get-map-geometry-bounds.md).
