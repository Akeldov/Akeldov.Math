# Data Storage

Akeldov.Math.Hexes provides storage types for values attached to hexes and for relationships
sampled over a regular Spatial2D raster. They build on the topology and geometry values described
in [Hex Grid Model](../hex-grid-model/index.md), but answer different lookup questions.

| Representation | Lookup direction | Typical use |
|---|---|---|
| Hex map | Hex index to one stored value | Terrain, costs, flags, labels, or application state |
| Neighborhood map | Hex index to the center and six adjacent indices | Repeated finite-grid neighborhood queries |
| Sampling raster | Raster sample to hex indices, interpolation weights, or chromatic classes | Repeated spatial sampling and interpolation |

## Maps

`IHexMap<TValue>` is the read-only contract for one value per cell of a
<xref:Akeldov.Math.Hexes.HexMapTopology>. `HexMap<TValue>` adds mutable row-major storage.
`ISpatialHexMap<TValue>` and `SpatialHexMap<TValue>` associate the same model with a
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>.

Boolean, integer, and floating-point values have topology-only and spatial specializations:

- `BoolHexMap` and `SpatialBoolHexMap` provide logical operators, morphology, and connectivity;
- `IntHexMap` and `SpatialIntHexMap` provide extrema, arithmetic, comparisons, and range methods;
- `FloatHexMap` and `SpatialFloatHexMap` add mixed numeric operations, generated noise, and blur.

Spatial operators retain geometry, while conversion methods create independent copies between
topology-only and spatial forms. See [Maps](maps.md) for ownership, compatibility, and operation
semantics.

## Complete and Partial Neighborhoods

Fixed-size containers preserve the meaning and order of related values:

- pairs describe two selected positions;
- triplets describe a main value and two vertex neighbors;
- sextuplets describe six edge neighbors without a center;
- septuplets describe a main value and all six edge neighbors.

Partial counterparts add presence flags for bounded domains. `SampleSextuplet` and
`SamplePartialSextuplet` read neighbor values directly from any `IHexMap<TValue>` using layout and
row or column parity. Precomputed septuplet maps instead store center and neighbor indices for
repeated lookup. See [Complete and Partial Neighborhoods](complete-and-partial-neighborhoods.md).

## Rasters

A storage raster has its own rectangular `RasterGeometry`. Each raster cell represents a sample
point and stores a precomputed relationship to a source hex-map geometry. Raster resolution is the
number of samples, not the number of source hexes.

- index rasters store the containing hex and selected neighbors;
- barycentric rasters store interpolation weights for a corresponding triplet;
- chromatic rasters store class indices or weights in a stable three-class order;
- complete rasters use infinite-grid coordinates, while partial rasters mark unavailable slots.

These are lookup rasters rather than rendered images. See [Rasters](rasters.md) for raster geometry,
indexing, validation, and paired index-and-weight usage.

## Choose a Storage Model

- Use a hex map when every cell has one application value.
- Sample a sextuplet when an algorithm needs current values of the six neighbors once.
- Use a neighborhood map when the same center-and-neighbor indices are read repeatedly.
- Use a sampling raster when the primary key is a point on a regular Spatial2D raster.
- Choose a partial form before indexing another bounded map.

Storage supplies data and precomputed relationships; algorithms decide how to consume them.
Continue with [Spatial Algorithms](../spatial-algorithms/index.md) for pathfinding, partitioning,
and chromatization.
