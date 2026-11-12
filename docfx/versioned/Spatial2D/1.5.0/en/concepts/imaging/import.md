# Import

<xref:Akeldov.Math.Spatial2D.Imaging.RasterImageLoader> loads PNG and BMP images into new mutable
`Raster<TColor>` buffers owned by the caller. Choose the input format and pixel type explicitly
through the method name. Each method accepts either a file path or a readable stream.

## Choose a loader

| Method | Supported input | Result |
|---|---|---|
| `LoadGray8FromPng` | 8-bit grayscale PNG without alpha | `Raster<Gray8BitColor>` |
| `LoadGray16FromPng` | 16-bit grayscale PNG without alpha | `Raster<Gray16BitColor>` |
| `LoadRgba8FromPng` | PNG with 8-bit RGBA channels | `Raster<RGBA8BitColor>` |
| `LoadRgba16FromPng` | PNG with 16-bit RGBA channels | `Raster<RGBA16BitColor>` |
| `LoadGray8FromBmp` | Uncompressed indexed 8-bit BMP with a grayscale palette | `Raster<Gray8BitColor>` |
| `LoadRgba8FromBmp` | Uncompressed indexed 8-bit BMP with any palette, or 32-bit BGRA BMP | `Raster<RGBA8BitColor>` |

PNG input must match both the requested color type and channel depth. The loader does not
automatically convert grayscale to RGBA or 16-bit channels to 8-bit channels. Indexed PNG,
RGB PNG without alpha, and grayscale PNG with alpha are not supported. All PNG scanline
filters and Adam7 interlacing are supported; samples are preserved without color-space conversion.

BMP loading supports bottom-up and top-down images with Windows information headers. Palette
indices are resolved to colors, not retained as cell values. Indexed colors are opaque; the
fourth byte of a 32-bit BGRA pixel is retained as alpha. There is no loader for 24-bit BMP or
compressed BMP.

Use the matching loader first, then `MapValues` for an explicit conversion between supported
cell types. See [Color Rasters](../rasterization/color-rasters.md) for channel ranges and precision.

## Load from a file or stream

The example expects `drawing.png` to contain 8-bit RGBA pixels, such as an image saved from
`SpatialRaster<RGBA8BitColor>` with `SaveAsPng`:

```csharp
using System.IO;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

Raster<RGBA8BitColor> image = RasterImageLoader.LoadRgba8FromPng("drawing.png");

using var stream = File.OpenRead("drawing.png");
Raster<RGBA8BitColor> fromStream = RasterImageLoader.LoadRgba8FromPng(stream);

var geometry = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(80f, 50f), image.Resolution);
var spatial = new SpatialRaster<RGBA8BitColor>(geometry, image.Values);
spatial.DrawText("Imported", new PointXY(10f, 40f), height: 3f);
spatial.SaveAsPng("annotated.png");
```

File overloads open and close their own input stream. Stream overloads start at its current
position and leave it open; seeking is not required. When reading a memory stream just used
for export, reset its `Position` to zero first. PNG reading stops after the `IEND` chunk;
BMP reading consumes the size declared in the file header.

## Orientation, ownership, and spatial bounds

Loaded rasters use Y increasing from the bottom image row: `image[0, 0]` is the bottom-left pixel.
Storage remains row-major, with flat index `y * Resolution.X + x`. The resulting pixel array
and indexers are mutable. Separate loading calls produce independent buffers.

An image supplies pixel resolution and colors, but does not restore `RasterGeometry` or a
numeric-to-color mapping used during export. The example assigns world bounds explicitly;
its `80` by `50` world-unit rectangle must come from the application, not from the PNG.

Wrapping `image.Values` in `SpatialRaster<TColor>` shares the same array: drawing through
`spatial` also changes `image`. Clone the array before construction if independent pixels are
needed. See [Spatial Rasters](../rasterization/spatial-rasters.md) for the geometry contract and
[Drawing](drawing.md) for operations on an imported spatial color raster.

## Unsupported or invalid input

Unsupported encodings and mismatched PNG pixel formats raise `NotSupportedException`.
Invalid, truncated, or oversized image data raises `InvalidDataException`. An unreadable input
stream raises `ArgumentException`; file access can also fail with the usual I/O exceptions.
Selecting a different method does not convert an unsupported source encoding.

Continue with [Export](export.md) for saving the result, or [Imaging](../imaging.md) for colors
and blending.
