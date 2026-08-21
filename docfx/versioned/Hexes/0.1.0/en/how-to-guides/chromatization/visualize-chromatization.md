# Visualize Chromatization

Render a `ChromaticIndexMap` with one color per class to inspect the three-color pattern. To inspect
interpolation near hex vertices, map chromatically ordered barycentric weights to the red, green,
and blue channels.

## Render the class map

Create a spatial chromatic map, rasterize it with a three-color palette, and save the result as PNG:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var chromaticMap = new ChromaticIndexMap(geometry);

SpatialRaster<RGBA8BitColor> classImage = chromaticMap.Rasterize(
    pixelsPerApothem: 24f,
    margin: geometry.Radius * 0.5f,
    colorSelector: ToClassColor);

string classImagePath = Path.GetFullPath("chromatic-classes.png");
classImage.SaveAsPng(classImagePath);

static RGBA8BitColor ToClassColor(byte chromaticIndex)
{
    return chromaticIndex switch
    {
        0 => new RGBA8BitColor(239, 71, 71, byte.MaxValue),
        1 => new RGBA8BitColor(59, 201, 114, byte.MaxValue),
        2 => new RGBA8BitColor(71, 119, 232, byte.MaxValue),
        _ => new RGBA8BitColor(0, 0, 0, 0)
    };
}
```

`Rasterize` selects the containing hex at the center of every output cell and passes its class to
`ToClassColor`. The result therefore has sharp boundaries between hexes. Cells in the outer margin
that map outside the finite topology keep `default(RGBA8BitColor)` and remain transparent.

Try the same code with `EvenR`, `OddQ`, or `EvenQ` to inspect how the offset layout changes the
pattern. The three-color invariant remains the same: cells with a shared edge have different
colors.

## Visualize class-ordered weights

Use <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster> to display smooth
weights around vertices. Assign class `0` to red, class `1` to green, and class `2` to blue:

```csharp
RasterGeometry rasterGeometry = geometry.ToRasterGeometry(
    pixelsPerApothem: 24f,
    margin: geometry.Radius * 0.5f);

var weightRaster = new ChromaticBarycentricPartialTripletRaster(
    geometry,
    rasterGeometry);

SpatialRaster<RGBA8BitColor> weightImage =
    weightRaster.MapValues(ToWeightColor);

string weightImagePath = Path.GetFullPath("chromatic-weights.png");
weightImage.SaveAsPng(weightImagePath);

static RGBA8BitColor ToWeightColor(
    PartialChromaticTriplet<float> weights)
{
    float red = weights.HasIndex0 ? weights.Index0 : 0f;
    float green = weights.HasIndex1 ? weights.Index1 : 0f;
    float blue = weights.HasIndex2 ? weights.Index2 : 0f;
    float sum = red + green + blue;

    return sum > 0f
        ? RGBA8BitColor.FromNormalized(
            red / sum,
            green / sum,
            blue / sum,
            alpha: 1f)
        : new RGBA8BitColor(0, 0, 0, 0);
}
```

`Index0`, `Index1`, and `Index2` are already reordered by chromatic class, so the same color channel
represents the same class throughout the image. Dividing by `sum` renormalizes boundary samples
whose neighboring hexes are only partially present. A sample with no in-map class becomes
transparent.

`MapValues` creates a new color raster while preserving `rasterGeometry`. The weight image is a
diagnostic view of the interpolation basis; it is not the same as rendering values stored in a hex
map.

## Interpret the output

- Use `chromatic-classes.png` to verify class assignment and layout.
- Use `chromatic-weights.png` to verify smooth class channels and finite-map boundaries.
- Increase `pixelsPerApothem` for finer output without changing the source geometry.
- Keep the palette fixed when comparing layouts or revisions.

See [Create a Chromatic Map](create-a-chromatic-map.md) and
[Create a Chromatic Raster](create-a-chromatic-raster.md) for the underlying data. General image
export options are covered in
[Convert a Raster to an Image](../rasters/convert-a-raster-to-an-image.md).
