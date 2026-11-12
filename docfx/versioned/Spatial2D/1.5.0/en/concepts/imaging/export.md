# Export

Export encodes a color raster as a PNG or BMP image. The extension methods
<xref:Akeldov.Math.Spatial2D.Imaging.RasterPngExtensions> and
<xref:Akeldov.Math.Spatial2D.Imaging.RasterBmpExtensions> accept
<xref:Akeldov.Math.Spatial2D.Rasterization.IRaster`1>, so both `Raster<TColor>` and
`SpatialRaster<TColor>` can be saved. Export reads the existing pixels without modifying them.

## Choose an image format

| Raster cell type | `SaveAsPng` | `SaveAsBmp` |
|---|---|---|
| `Gray8BitColor` | 8-bit grayscale | 8-bit indexed grayscale |
| `Gray16BitColor` | 16-bit grayscale | No overload |
| `RGBA8BitColor` | 8 bits per RGBA channel | 32-bit BGRA |
| `RGBA16BitColor` | 16 bits per RGBA channel | No overload |

PNG preserves the selected channel depth and RGBA alpha. Saving a 16-bit color raster does not
reduce its channels to 8 bits. BMP requires one of the two supported 8-bit color types; convert
values explicitly when exporting a higher-precision raster to BMP.

The export method determines the encoding, not the filename extension. Use a matching `.png`
or `.bmp` extension so other software can identify the file easily. See
[Color Rasters](../rasterization/color-rasters.md) for the pixel types and their channel ranges.

## Prepare numeric values

Numeric and Boolean rasters need an explicit mapping to a supported color type before export.
Use `MapValues` to create a new color buffer. A Boolean mask can map `false` to black and `true`
to white. For heights or distances, choose the physical range first and normalize it to `[0, 1]`.

`FromNormalized` clamps and rounds color channels. Values outside the selected range lose
information, and a zero-width range needs a separate mapping. Retain the numeric raster when
later calculations require the original values.

## Save to a file or stream

This example maps heights in `[0, 10]` to grayscale, writes a 16-bit PNG, and creates an 8-bit
BMP preview. The same PNG can also be encoded into a memory stream:

```csharp
using System.IO;
using System.IO.Compression;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var heights = new FloatRaster(
    new VectorXYInt(2, 2), new[] { 0f, 2.5f, 5f, 10f });
Raster<Gray16BitColor> image = heights.MapValues(
    height => Gray16BitColor.FromNormalized(height / 10f));
image.SaveAsPng("heights.png");

Raster<Gray8BitColor> preview = heights.MapValues(
    height => Gray8BitColor.FromNormalized(height / 10f));
preview.SaveAsBmp("heights.bmp");

using var stream = new MemoryStream();
image.SaveAsPng(stream, CompressionLevel.Fastest);
byte[] pngBytes = stream.ToArray();
```

Path overloads create or overwrite the target file. Stream overloads write to a caller-supplied
writable stream and leave it open; the caller owns its lifetime. Reset `Position` to zero on a
seekable stream before reading the encoded image from that stream.

## PNG compression and spatial metadata

PNG uses `CompressionLevel.Optimal` by default. Overloads accepting `CompressionLevel` allow
a different compression setting, such as `Fastest`. Compression changes encoding cost and file
size, not pixel values or channel precision: PNG compression is lossless.

Image output contains pixel resolution and colors, not `RasterGeometry`, world bounds, or the
numeric-to-color mapping. Store that information separately when the image must map back to
world coordinates or physical values. Exporting a spatial raster does not remove its in-memory
geometry; it simply does not encode that geometry in the image.

Continue with [Imaging](../imaging.md), [Drawing](drawing.md), or the
[Exporting the Image](../../tutorials/building-an-influence-map/exporting-the-image.md) tutorial.
