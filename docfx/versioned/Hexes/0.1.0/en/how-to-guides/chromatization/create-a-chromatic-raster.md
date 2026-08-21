# Create a Chromatic Raster

Use chromatic rasters to precompute the three-color classes or class-ordered interpolation weights
around every rectangular raster sample. They preserve a stable relationship between classes `0`,
`1`, and `2` even when the geometric `Main`, `Left`, and `Right` order changes across the grid.

## Define one sampling geometry

Create a source hex geometry and the rectangular grid on which it will be sampled:

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
    pixelsPerApothem: 16f,
    margin: mapGeometry.Radius);
```

Reuse this exact `RasterGeometry` for every related raster. Equal resolutions alone do not align
samples when origins or world-space sizes differ.

## Read classes in geometric order

Create <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexTripletRaster> to classify the containing hex
and the two neighbors that meet it at the closest vertex:

```csharp
var classRaster = new ChromaticIndexTripletRaster(
    mapGeometry,
    rasterGeometry);

var sample = new VectorXYInt(
    classRaster.Resolution.X / 2,
    classRaster.Resolution.Y / 2);

if (classRaster.TryGetValue(sample, out Triplet<byte> classes))
{
    Console.WriteLine($"Main:  class {classes.Main}");
    Console.WriteLine($"Left:  class {classes.Left}");
    Console.WriteLine($"Right: class {classes.Right}");
}
```

`Main`, `Left`, and `Right` describe geometric positions, not fixed class numbers. For a complete
vertex triplet, their values are a permutation of `0`, `1`, and `2`; do not assume that `Main` is
class `0`.

The complete raster classifies the implied infinite grid. `TryGetValue` checks only the rectangular
raster coordinate, so a returned class can belong to a hex outside the finite source topology.

## Read barycentric weights in class order

Create <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster> when one channel must
always represent one chromatic class:

```csharp
var weightRaster = new ChromaticBarycentricTripletRaster(
    mapGeometry,
    rasterGeometry);

if (weightRaster.TryGetValue(
        sample,
        out ChromaticTriplet<float> weights))
{
    Console.WriteLine($"Class 0 weight: {weights.Index0}");
    Console.WriteLine($"Class 1 weight: {weights.Index1}");
    Console.WriteLine($"Class 2 weight: {weights.Index2}");
}
```

These are the ordinary barycentric `Main`, `Left`, and `Right` weights reordered by the classes in
`classRaster`. `Index0`, `Index1`, and `Index2` therefore always correspond to classes `0`, `1`, and
`2`, respectively, and sum to approximately `1` for a complete triplet.

The two examples intentionally expose different orders: `classRaster.Main` identifies the class of
the main geometric hex, while `weightRaster.Index0` is the weight of whichever geometric hex has
class `0`. Do not combine the components position by position.

## Handle the finite boundary

Use the partial variants when only source-map cells are valid:

```csharp
var partialClassRaster = new ChromaticIndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);

var partialWeightRaster = new ChromaticBarycentricPartialTripletRaster(
    mapGeometry,
    rasterGeometry);

PartialTriplet<byte> partialClasses = partialClassRaster[sample];
PartialChromaticTriplet<float> partialWeights = partialWeightRaster[sample];

if (partialClasses.HasMain)
    Console.WriteLine($"Main class: {partialClasses.Main}");
if (partialClasses.HasLeft)
    Console.WriteLine($"Left class: {partialClasses.Left}");
if (partialClasses.HasRight)
    Console.WriteLine($"Right class: {partialClasses.Right}");

if (partialWeights.HasIndex0)
    Console.WriteLine($"Present class 0 weight: {partialWeights.Index0}");
if (partialWeights.HasIndex1)
    Console.WriteLine($"Present class 1 weight: {partialWeights.Index1}");
if (partialWeights.HasIndex2)
    Console.WriteLine($"Present class 2 weight: {partialWeights.Index2}");
```

The class raster keeps presence in geometric order with `HasMain`, `HasLeft`, and `HasRight`. The
barycentric raster reorders both values and presence into `HasIndex0`, `HasIndex1`, and `HasIndex2`.
Absent weights are not renormalized; divide by the sum of present weights when blending bounded-map
values.

`ChromaticIndexPartialTripletRaster` has no `TryGetValue`; validate the coordinate
or use its checked `[VectorXYInt]` indexer. `ChromaticBarycentricPartialTripletRaster.TryGetValue`
returns `false` only for a coordinate outside the rectangular raster. A successful call can still
return a value whose three presence flags are clear.

## Choose the matching raster

| Required data | Complete source grid | Finite source map |
|---|---|---|
| Classes in `Main/Left/Right` order | `ChromaticIndexTripletRaster` | `ChromaticIndexPartialTripletRaster` |
| Weights in `Index0/Index1/Index2` order | `ChromaticBarycentricTripletRaster` | `ChromaticBarycentricPartialTripletRaster` |

The one-argument constructors automatically cover the map at one pixel per apothem with no margin.
Use explicit sampling geometry when multiple layers must align. Construction eagerly calculates all
cells, so reuse these rasters while the source and sampling geometries remain unchanged.

Continue with [Visualize Chromatization](visualize-chromatization.md), or see
[Create a Barycentric Raster](../rasters/create-a-barycentric-raster.md) for geometric-order
interpolation. The complete ordering rules are described in
[Chromatization](../../concepts/spatial-algorithms/chromatization.md).
