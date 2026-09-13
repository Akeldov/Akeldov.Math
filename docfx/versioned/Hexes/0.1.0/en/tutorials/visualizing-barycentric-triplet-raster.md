# Visualizing BarycentricTripletRaster

Display the main barycentric weight as a grayscale image.
<xref:Akeldov.Math.Hexes.Topology.BarycentricTripletRaster> stores three interpolation weights
at the center of every raster cell. This example reproduces the main-weight visualization from
the geometry raster examples.

## From indices to weights

Like [IndexTripletRaster](visualizing-index-triplet-raster.md), the raster finds the containing
hex and its closest vertex. The containing hex and the two neighbors meeting it at that vertex
supply three **hex centers** that form an interpolation triangle. Its vertices are not the
corners of an individual hex.

An index raster stores a `Triplet<VectorXYInt>`; this raster stores a `Triplet<float>`.
The `Main`, `Left`, and `Right` components are weights for the corresponding hex centers:

```text
P = Main * centerMain + Left * centerLeft + Right * centerRight
Main + Left + Right ≈ 1
```

The weights vary with the sampled point even while its three selected hex indices remain
unchanged. Their order is geometric, not chromatic: `Main` is not tied to a fixed color class.

## Render the main weight

Run this complete example in a console project that references Akeldov.Math.Hexes.
See [Creating a Project](creating-a-hex-map/creating-a-project.md) for setup.

The source is a 5 × 4 `OddR` map whose zero hex is centered at world point `(0, 0)`.
Its center-to-vertex radius is one world unit. `ToRasterGeometry` covers the map's bounding
rectangle at 16 pixels per apothem with no additional margin; its origin need not equal
the zero hex's center.

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

var sourceRaster = new BarycentricTripletRaster(
    hexGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("barycentric-triplet-raster-main-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(Triplet<float> barycentric)
{
    float main = barycentric.Main;
    return RGBA16BitColor.FromNormalized(main, main, main);
}
```

![BarycentricTripletRaster with the Main weight displayed in grayscale](~/assets/hexes/rasters/barycentric-triplet-raster-main-odd-r-rgba16.png)

## Read the image

Equal red, green, and blue channels produce grayscale. `FromNormalized` converts `Main` to
16-bit channels and uses full opacity by default. `MapValues` preserves the sampling geometry.

- At a hex center, the theoretical weights are `(1, 0, 0)`: the main weight is brightest.
- Moving toward the hex boundary reduces the main weight.
- At a hex vertex, the three surrounding centers contribute equally: each weight is `1/3`.

Raster samples lie at pixel centers, so the brightest sampled value need not be exactly `1`.
Within each selected triangle the weights vary linearly. The image shows the weight of whichever
hex currently occupies the `Main` role, not a height map, a blur, or one fixed hex's influence.

## Boundaries and interpolation

This is the **complete** raster: it computes weights on the implied infinite hex grid, even
when one or more corresponding hex indices lie outside the finite source map. The rectangular
image therefore stays opaque, including samples outside the map's jagged outline.

For actual map-value interpolation, use an `IndexTripletRaster` built with the same
`HexMapGeometry` and `RasterGeometry`, then combine corresponding values and weights.
Matching only the raster resolution is not enough to align the samples. Do not use unchecked
out-of-map indices to read a bounded `HexMap<T>`.

For finite-map boundaries, pair `IndexPartialTripletRaster` with
`BarycentricPartialTripletRaster` and normalize the remaining weights when appropriate.
See [Create a Barycentric Raster](../how-to-guides/rasters/create-a-barycentric-raster.md)
for a worked interpolation example.

To visualize all three weights with stable RGB class channels instead of geometric
`Main/Left/Right` roles, continue with
[Chromatization and Barycentric Interpolation](chromatic-barycentric-interpolation.md).
