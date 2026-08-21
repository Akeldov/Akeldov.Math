# Create an Index Septuplet Raster

Use <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletRaster> to precompute a central hex index and
all six of its edge-adjacent neighbors for every raster sample. This seven-index lookup is useful
for neighborhood filters, local simulations, and other operations that need every immediate
neighbor of the containing hex.

## Create the raster

Define the finite hex map in world space, derive a sampling grid, and pass both geometries to the
raster:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var mapGeometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: 16f);

var indexRaster = new IndexSeptupletRaster(
    mapGeometry,
    rasterGeometry);
```

`pixelsPerApothem` controls the sampling density. Reuse the same `rasterGeometry` when this lookup
must align with another raster. The shorter `new IndexSeptupletRaster(mapGeometry)` overload
creates a grid at one pixel per apothem with no margin.

`SourceHexMapGeometry` exposes the source map geometry, `Geometry` exposes the sampling grid, and
`Resolution` gives the number of raster cells. Read the source topology through
`indexRaster.SourceHexMapGeometry.Topology`.

## Read a seven-index neighborhood

The center raster cell is guaranteed to be inside the positive resolution created above:

```csharp
var sample = new VectorXYInt(
    indexRaster.Resolution.X / 2,
    indexRaster.Resolution.Y / 2);

Septuplet<VectorXYInt> neighborhood = indexRaster[sample];

Console.WriteLine($"Main:      {neighborhood.Main}");
Console.WriteLine($"Adjacent0: {neighborhood.Adjacent0}");
Console.WriteLine($"Adjacent1: {neighborhood.Adjacent1}");
Console.WriteLine($"Adjacent2: {neighborhood.Adjacent2}");
Console.WriteLine($"Adjacent3: {neighborhood.Adjacent3}");
Console.WriteLine($"Adjacent4: {neighborhood.Adjacent4}");
Console.WriteLine($"Adjacent5: {neighborhood.Adjacent5}");
```

`Main` is the containing or nearest hex on the implied infinite grid. `Adjacent0` through
`Adjacent5` are its six edge neighbors and correspond to `HexEdge.Edge0` through `HexEdge.Edge5`.
Their physical directions depend on the map layout, but the edge-number order remains stable.

The same value can be read with `[x, y]` or a row-major flat integer index. In version `0.1.0`,
the septuplet raster has no `TryGetValue` method. Prefer the `[VectorXYInt]` indexer when indices
are not compile-time constants: it checks both coordinates and throws `IndexOutOfRangeException`
when they lie outside `Resolution`.

## Choose septuplets or triplets

An `IndexSeptupletRaster` always selects `Main` and all six edge-adjacent neighbors. An
`IndexTripletRaster` instead selects `Main` and the two neighbors that meet it at the closest
vertex. Use a septuplet for a complete one-ring neighborhood; use a triplet when an operation,
such as barycentric interpolation, depends on one vertex and its three surrounding hexes.

## Handle the finite-map boundary

The complete septuplet describes the infinite hex grid. Near the source-map boundary, some
adjacent indices can lie outside the finite topology, and even `Main` can be outside when the
sampling geometry includes a margin. Do not use those indices directly with a bounded
`HexMap<T>`.

For boundary-aware values, create an
<xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster> with the same geometries:

```csharp
var partialIndexRaster = new IndexPartialSeptupletRaster(
    mapGeometry,
    rasterGeometry);
```

Its presence flags identify which positions belong to the source topology. Continue with
[Handle Partial Neighborhoods](handle-partial-neighborhoods.md) before indexing a finite map.
For a three-index vertex lookup, see
[Create an Index Triplet Raster](create-an-index-triplet-raster.md). The complete raster model is
described in [Rasters](../../concepts/data-storage/rasters.md).
