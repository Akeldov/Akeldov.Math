# Visualizing ChromaticIndexTripletRaster

Encode the chromatic classes of three neighboring hexes in RGB channels.
<xref:Akeldov.Math.Hexes.Topology.ChromaticIndexTripletRaster> stores a `Triplet<byte>`
at every raster sample. Its components are class numbers `0`, `1`, and `2`, not hex
coordinates or interpolation weights.

This example reproduces the class-triplet image from the chromatic raster examples.
For the three-color classification itself, see
[Chromatizing a Hex Map](chromatizing-a-hex-map.md).

## Classes in geometric order

At the world-space center of each pixel, the raster finds the containing hex and its closest
vertex. It stores the classes of that hex and the two neighbors meeting it at the vertex:

- `Main`: the containing hex's class, encoded in red.
- `Left`: one neighbor's class, encoded in green.
- `Right`: the other neighbor's class, encoded in blue.

These three hexes are pairwise edge-adjacent, so their classes are distinct. Every triplet
is a permutation of `(0, 1, 2)`. The component order is geometric: `Main` need not be
class `0`, and `Left` and `Right` are not fixed screen directions.

Unlike `ChromaticIndexMap`, which stores one class per hex, this raster stores three
classes per rectangular raster cell.

## Render the class triplets

Run this complete example in a console project that references Akeldov.Math.Hexes.
See [Creating a Project](creating-a-hex-map/creating-a-project.md) for setup.

The source contains 5 × 4 hexes in the `OddR` layout, with a center-to-vertex radius of one
world unit. The constructor without an explicit origin places the zero hex at
`(apothem, radius)` for this layout. The raster starts at world point `(0, 0)`, spans the
map's bounding-box size, and samples it at 192 × 192 pixels.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.IO;

var topology = new HexMapTopology(5, 4, Layout.OddR);
var hexMapGeometry = new HexMapGeometry(topology, radius: 1f);

var rasterGeometry = new RasterGeometry(
    new PointXY(0f, 0f),
    hexMapGeometry.GetBoundingBoxSize(),
    new VectorXYInt(192, 192));

var sourceRaster = new ChromaticIndexTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("chromatic-index-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(Triplet<byte> chromatic)
{
    return new RGBA16BitColor(
        ToChannel(0.18f + 0.34f * chromatic.Main),
        ToChannel(0.18f + 0.34f * chromatic.Left),
        ToChannel(0.18f + 0.34f * chromatic.Right),
        ushort.MaxValue);
}

static ushort ToChannel(float value)
{
    value = MathF.Min(MathF.Max(value, 0f), 1f);
    return (ushort)MathF.Round(value * ushort.MaxValue);
}
```

![ChromaticIndexTripletRaster with Main, Left, and Right class numbers encoded in RGB](~/assets/hexes/rasters/chromatic-index-triplet-raster-odd-r-rgba16.png)

## Read the colors

The formula `0.18f + 0.34f * classIndex` maps classes `0`, `1`, and `2` to normalized
intensities `0.18`, `0.52`, and `0.86`. `ToChannel` clamps and converts each intensity
to a 16-bit channel; alpha is always fully opaque.

Each pixel therefore uses one of six permutations of these three channel intensities.
The color stays constant while the class triplet stays unchanged. The repeating, sharply
separated regions show class ordering, not smooth interpolation.

Here, red always encodes the class of `Main`; it does **not** mean “class 0 contributes to
red.” Likewise, green and blue encode the class numbers at `Left` and `Right`.
`MapValues` performs only this value-to-color conversion and preserves the sampling geometry.

## Boundaries and barycentric weights

This is the **complete** raster: it classifies the implied infinite grid, including hexes
outside the finite source topology. It does not add presence flags or clip the image to the
map's outline. Class `0` is a valid class, not an absence marker.

Use `ChromaticIndexPartialTripletRaster` and its presence flags when only in-map hexes may
participate. See [Create a Chromatic Raster](../how-to-guides/chromatization/create-a-chromatic-raster.md)
for boundary-aware class lookup.

To blend values, class numbers alone are not enough: you also need barycentric weights.
`BarycentricTripletRaster` stores weights in geometric `Main/Left/Right` order, while
`ChromaticBarycentricTripletRaster` reorders them by class into `Index0/Index1/Index2`.
Those class-ordered components are **weights**, not the class numbers displayed here.

Continue with [Chromatization and Barycentric Interpolation](chromatic-barycentric-interpolation.md)
for a smooth RGB visualization using one stable color channel per class.
