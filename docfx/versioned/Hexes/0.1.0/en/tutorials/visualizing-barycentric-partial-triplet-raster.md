# Visualizing BarycentricPartialTripletRaster

Display the main barycentric weight only where its hex belongs to the finite source map.
<xref:Akeldov.Math.Hexes.Topology.BarycentricPartialTripletRaster> stores a
`PartialTriplet<float>` at each raster sample: three geometric weights and their presence flags.

This example uses the same geometry and grayscale mapping as
[Visualizing BarycentricTripletRaster](visualizing-barycentric-triplet-raster.md).
It reproduces the partial main-weight image from the geometry raster examples.

## Weights and presence

The raster selects the containing hex (`Main`) and the two neighbors (`Left` and `Right`)
meeting it at its closest vertex. The three **hex centers** form the interpolation triangle,
just as in the complete raster.

`HasMain`, `HasLeft`, and `HasRight` indicate which of these hexes belong to the source
topology. Present positions retain their original barycentric weights; absent positions contain
zero. The remaining weights are **not automatically normalized**, so their sum can be less
than one at the boundary.

The components contain weights, not indices. Use a matching `IndexPartialTripletRaster`
when you also need to look up values stored in the corresponding hexes.

## Render the present main weight

Run this complete example in a console project that references Akeldov.Math.Hexes.
See [Creating a Project](creating-a-hex-map/creating-a-project.md) for setup.

The source is a 5 × 4 `OddR` map with its zero hex centered at world point `(0, 0)` and a
center-to-vertex radius of one world unit. The sampling grid covers the map's bounding rectangle
at 16 pixels per apothem without an additional margin. The rectangle still contains points
outside the map's jagged outline.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

var hexGeometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

RasterGeometry rasterGeometry = hexGeometry.ToRasterGeometry(
    pixelsPerApothem: 16f,
    margin: 0f);

var sourceRaster = new BarycentricPartialTripletRaster(
    hexGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("barycentric-partial-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(PartialTriplet<float> barycentric)
{
    float main = barycentric.HasMain ? barycentric.Main : 0f;
    return RGBA16BitColor.FromNormalized(main, main, main);
}
```

![BarycentricPartialTripletRaster with present Main weights in grayscale and absent Main positions in black](~/assets/hexes/rasters/barycentric-partial-triplet-raster-odd-r-rgba16.png)

## Compare with the complete raster

Where `HasMain` is true, the brightness matches the complete main-weight image, even if
`Left` or `Right` is absent. The example displays the original `Main` weight without
redistributing missing neighbors' contributions.

Where `HasMain` is false, `ToColor` uses zero intensity. Because `FromNormalized` uses
full opacity by default, these pixels are **opaque black**, not transparent. A present
`Left` or `Right` cannot make such a pixel brighter: this visualization deliberately
ignores both of those weights.

Thus the image shows the finite map's outline while retaining the bright hex centers and
darker edges of the complete example. `MapValues` preserves the sampling geometry; clipping
the displayed weight does not crop the rectangular raster.

## Normalize only when the operation needs it

For interpolation over a finite map, combine only present source values and divide by the
sum of their weights when that sum is positive:

```text
weightSum = sum of present weights
value = sum of (present weight * corresponding map value) / weightSum
```

This preserves a constant source field near the boundary. If `weightSum` is zero, choose an
explicit no-data result. The grayscale example above intentionally does not perform this
normalization: it visualizes the stored main weight itself.

Build the index and weight rasters with the same `HexMapGeometry` and `RasterGeometry`.
Check presence flags before reading a bounded map; a numerical weight of zero is not a
substitute for a presence flag.

See [Create a Barycentric Raster](../how-to-guides/rasters/create-a-barycentric-raster.md)
for a complete boundary-safe interpolation example, or
[Visualizing IndexPartialTripletRaster](visualizing-index-partial-triplet-raster.md)
to display the corresponding indices instead of weights.
