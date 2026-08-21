# Create a Barycentric Raster

Use <xref:Akeldov.Math.Hexes.Topology.BarycentricTripletRaster> to precompute the three interpolation
weights at the center of every raster cell. Its `Main`, `Left`, and `Right` weights correspond to
the same positions in an `IndexTripletRaster`; combine the two rasters to interpolate values stored
at hex centers.

## Create matching lookup rasters

Define the finite source map and one sampling geometry. For a bounded map, create the partial
variants so samples at its boundary never expose an out-of-range hex index:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var mapGeometry = new HexMapGeometry(
    width: 3,
    height: 2,
    radius: 1f,
    layout: Layout.OddR);

var values = new HexMap<float>(mapGeometry.Topology, new[]
{
    10f, 20f, 30f,
    40f, 50f, 60f
});

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: 8f,
    margin: mapGeometry.Radius);

var indexRaster = new IndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);

var weightRaster = new BarycentricPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Both lookup rasters must receive the same `HexMapGeometry` and `RasterGeometry`. Matching only their
resolutions is not enough: the origin and world-space size determine the point sampled by each
raster cell. With shared geometries, the `Main`, `Left`, and `Right` positions and weights line up.

The shorter `new BarycentricPartialTripletRaster(mapGeometry)` overload derives a sampling grid at
one pixel per apothem with no margin. Use an explicit geometry whenever this raster must align with
another raster or output image.

## Interpolate a sample

Read the index and weight triplets at the same raster coordinate, accumulate only present positions,
and normalize their remaining weights:

```csharp
var sample = new VectorXYInt(
    weightRaster.Resolution.X / 2,
    weightRaster.Resolution.Y / 2);

if (indexRaster.TryGetValue(sample, out PartialTriplet<VectorXYInt> indices) &&
    weightRaster.TryGetValue(sample, out PartialTriplet<float> weights))
{
    float interpolated = Interpolate(values, indices, weights);
    Console.WriteLine($"Interpolated value: {interpolated}");
}

static float Interpolate(
    HexMap<float> map,
    PartialTriplet<VectorXYInt> indices,
    PartialTriplet<float> weights)
{
    float weightedValue = 0f;
    float weightSum = 0f;

    if (indices.HasMain)
    {
        weightedValue += map[indices.Main] * weights.Main;
        weightSum += weights.Main;
    }

    if (indices.HasLeft)
    {
        weightedValue += map[indices.Left] * weights.Left;
        weightSum += weights.Left;
    }

    if (indices.HasRight)
    {
        weightedValue += map[indices.Right] * weights.Right;
        weightSum += weights.Right;
    }

    return weightSum > 0f
        ? weightedValue / weightSum
        : float.NaN;
}
```

For an interior sample, all three positions are present and their weights sum to approximately `1`.
At a finite-map boundary, `BarycentricPartialTripletRaster` clears the absent positions but preserves
the original weights of the present ones. Dividing by `weightSum` renormalizes that partial
neighborhood and prevents values from fading toward zero at the edge.

Because matching partial index and weight rasters calculate the same presence flags, checking the
index triplet's `HasMain`, `HasLeft`, and `HasRight` properties is sufficient. A successful
`TryGetValue` means that the raster coordinate exists and at least one source-map position is
present. It returns `false` for an out-of-range raster coordinate or a sample with no in-map weight.

## Use the complete variant when appropriate

Create `BarycentricTripletRaster` together with `IndexTripletRaster` when all three positions on the
implied infinite hex grid are meaningful to the consumer:

```csharp
var completeIndices = new IndexTripletRaster(mapGeometry, rasterGeometry);
var completeWeights = new BarycentricTripletRaster(mapGeometry, rasterGeometry);

Triplet<VectorXYInt> indices = completeIndices[sample];
Triplet<float> weights = completeWeights[sample];
```

The complete raster always returns three weights for a valid raster cell, even when a matching hex
index lies outside the finite source topology. Its `TryGetValue` checks only the raster coordinate.
Do not use unchecked complete indices to read a bounded `HexMap<T>`.

Construction performs all spatial classification eagerly. Reuse the lookup rasters when map values
change but both geometries remain unchanged. See [Create an Index Triplet Raster](create-an-index-triplet-raster.md)
for index semantics and [Handle Partial Neighborhoods](handle-partial-neighborhoods.md) for boundary
handling. The underlying storage model is described in
[Rasters](../../concepts/data-storage/rasters.md).
