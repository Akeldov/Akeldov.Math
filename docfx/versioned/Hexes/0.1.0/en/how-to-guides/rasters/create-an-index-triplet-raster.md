# Create an Index Triplet Raster

Use <xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster> to precompute the three hex indices that
surround the sample point at the center of every raster cell. The result is lookup data for
interpolation and other spatial operations; it is not an image by itself.

## Define the source and sampling geometries

Create a `HexMapGeometry` for the finite source map, then derive a `RasterGeometry` that covers it:

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

var indexRaster = new IndexTripletRaster(
    mapGeometry,
    rasterGeometry);
```

`pixelsPerApothem` controls the sampling density. Keep the resulting `rasterGeometry` when several
specialized rasters must line up cell for cell. The shorter
`new IndexTripletRaster(mapGeometry)` overload instead creates a grid at one pixel per apothem
with no margin.

The raster exposes the supplied geometries through `SourceHexMapGeometry` and `Geometry`.
`Resolution` is the raster-cell resolution, while `Topology` is the source hex-map topology;
these are different coordinate spaces.

## Read a triplet

Choose a raster-cell index and call `TryGetValue` before reading the result:

```csharp
var sample = new VectorXYInt(
    indexRaster.Resolution.X / 2,
    indexRaster.Resolution.Y / 2);

if (indexRaster.TryGetValue(
        sample,
        out Triplet<VectorXYInt> hexIndices))
{
    Console.WriteLine($"Main:  {hexIndices.Main}");
    Console.WriteLine($"Left:  {hexIndices.Left}");
    Console.WriteLine($"Right: {hexIndices.Right}");
}
```

For each sample point:

- `Main` is the containing or nearest hex on the implied infinite grid.
- `Left` and `Right` are the two neighbors that meet `Main` at the closest vertex.
- The left/right order is relative to that vertex and the selected layout.

The same values can be read with the `[x, y]`, `[VectorXYInt]`, or row-major flat integer indexer.
`TryGetValue` is convenient when the raster-cell coordinates may be outside `Resolution`.

## Handle the finite-map boundary

`IndexTripletRaster` models the complete infinite hex grid. Near the edge of the finite source
map, any of `Main`, `Left`, or `Right` can therefore lie outside `Topology.Resolution`.
`TryGetValue` checks only whether the requested raster cell exists; it does not guarantee that the
three returned hex indices belong to the finite map.

Do not use an unchecked triplet to index a bounded `HexMap<T>`. When only in-map indices may be
consumed, create an <xref:Akeldov.Math.Hexes.Topology.IndexPartialTripletRaster> with the same
geometries and inspect its presence flags:

```csharp
var partialIndexRaster = new IndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Continue with [Handle Partial Neighborhoods](handle-partial-neighborhoods.md) for boundary-safe
lookups. To calculate interpolation weights for the same three positions, reuse `rasterGeometry`
in [Create a Barycentric Raster](create-a-barycentric-raster.md). For the complete raster model,
see [Rasters](../../concepts/data-storage/rasters.md).
