# Data Storage

Akeldov.Math.Hexes provides storage types for values attached to hexes and for relationships
sampled over a regular Spatial2D raster. They build on the topology and geometry values described
in [Hex Grid Model](../hex-grid-model/index.md), but answer different lookup questions.

| Representation | Lookup direction | Typical use |
|---|---|---|
| Hex map | Hex index to one stored value | Terrain, costs, flags, labels, or application state |
| Neighborhood map | Hex index to the center and its six adjacent indices | Repeated finite-grid neighborhood queries |
| Sampling raster | Raster sample to hex indices, interpolation weights, or chromatic classes | Repeated spatial sampling and interpolation |

These types do not replace one another. A map stores per-cell values or exposes access to them,
while a raster precomputes how regularly spaced sample points relate to a source hex grid.

## Maps

`IHexMap<TValue>` is the read-only contract for one value per cell of a
<xref:Akeldov.Math.Hexes.HexMapTopology>. `HexMap<TValue>` adds mutable indexers and stores values
in row-major order. Its `VectorXYInt` indexer expresses the X/Y coordinates explicitly; its flat
indexer is useful when an algorithm already traverses the underlying storage order.

`SpatialHexMap<TValue>` associates the same storage model with a
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>. Use it when a consumer needs both stored values
and the physical placement of their cells. Hexes 0.2.0 also provides `BoolHexMap`, `IntHexMap`, and
`FloatHexMap` for cell-wise logic, numeric operations, generated noise, and filtering.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var terrain = new HexMap<string?>(topology);
terrain[new VectorXYInt(1, 1)] = "forest";
```

See [Maps](maps.md) for construction, array ownership, indexing, typed maps, and spatial maps.

## Rasters

A storage raster has its own rectangular `RasterGeometry`. Each raster cell represents a sample
point and stores a precomputed relationship to a source hex-map geometry.
The raster resolution therefore describes the number of samples, not the number of hexes.

The raster families answer related but distinct questions:

- index rasters store the containing hex and selected neighbors;
- barycentric rasters store interpolation weights for a corresponding triplet;
- chromatic rasters store class indices or weights in a stable three-class order;
- triplet rasters describe the three hexes around the closest grid vertex, while septuplet
  rasters describe a containing hex and all six edge-adjacent neighbors.

These are lookup rasters rather than rendered images. They are useful when the same sampling grid
will be queried repeatedly; image-producing rasterization APIs are described separately in
[Rasterization](../rasterization.md).

See [Rasters](rasters.md) for raster geometry, value families, indexing, validation, and paired
index-and-weight usage.

## Complete and Partial Neighborhoods

Fixed-size `Pair<T>`, `Triplet<T>`, and `Septuplet<T>` values always contain all logical slots.
For indices near the boundary of a finite map, a complete neighborhood can consequently contain
valid infinite-grid indices that lie outside that map.

`PartialPair<T>`, `PartialTriplet<T>`, and `PartialSeptuplet<T>` add presence flags. Partial maps
and rasters preserve slot meaning and order while marking entries that are unavailable inside the
finite source domain. They are the safer choice before indexing another bounded map.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);

var neighborhoods = new IndexPartialSeptupletMap(topology);
var corner = neighborhoods[new VectorXYInt(0, 0)];

if (corner.HasAdjacent0)
{
    VectorXYInt neighbor = corner.Adjacent0;
}
```

See [Complete and Partial Neighborhoods](complete-and-partial-neighborhoods.md) for slot order,
presence masks, boundary behavior, and safe access patterns.

## Choose a Storage Model

- Use a hex map when the primary key is a cell index and every cell has one value.
- Use a neighborhood map when the same six-neighbor relation is read many times per hex.
- Use a sampling raster when the primary key is a point on a regular Spatial2D raster.
- Choose a complete form when infinite-grid neighbors are meaningful and the consumer handles
  bounds itself.
- Choose a partial form when values will be used directly against a finite map.

Storage supplies data and precomputed relationships; algorithms decide how to consume them.
Continue with [Spatial Algorithms](../spatial-algorithms/index.md) for pathfinding, partitioning,
and chromatization built on these representations.
