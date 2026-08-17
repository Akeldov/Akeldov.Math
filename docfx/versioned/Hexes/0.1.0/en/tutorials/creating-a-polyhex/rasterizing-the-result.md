# Rasterizing the Result

In this final part of the tutorial, you will sample the region's signed distance on a regular
grid. Pixels inside the polyhex become white; the background and the hole remain black.

## Create and save the raster

Add these imports at the top of `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
```

Then add this code after constructing `region`:

```csharp
static Gray8BitColor ToMaskColor(float signedDistance) =>
    signedDistance <= 0f ? Gray8BitColor.White : Gray8BitColor.Black;

var rasterGeometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(9f, 7f),
    resolution: new VectorXYInt(720, 560));

SpatialRaster<Gray8BitColor> raster =
    region.Rasterize(ToMaskColor, rasterGeometry);

string outputPath = Path.GetFullPath("polyhex.png");
raster.SaveAsPng(outputPath);

Console.WriteLine(
    $"Raster: {raster.Resolution.X} x {raster.Resolution.Y}");
Console.WriteLine($"Saved: {outputPath}");
```

The final output includes:

```text
Raster: 720 x 560
Saved: <project directory>\polyhex.png
```

<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> maps the `9`-by-`7` coordinate-space
rectangle to 720 by 560 pixels. `Rasterize` passes each pixel's signed distance to `ToMaskColor`:
a non-positive value is inside the even-odd region, while a positive value is outside or inside
the hole.

Open `polyhex.png` to see the white 11-hex shape with its black central hole. For dynamically
sized masks, calculate a grid from the generated contour bounds instead of using this example's
known dimensions.

You now have an immutable polyhex, cell-level geometry, a filled Spatial2D region, and a PNG
representation. Continue with the [Polyhexes concept](../../concepts/hex-grid-model/polyhexes.md)
for builders, extension and contour masks, ownership rules, and validation details.
