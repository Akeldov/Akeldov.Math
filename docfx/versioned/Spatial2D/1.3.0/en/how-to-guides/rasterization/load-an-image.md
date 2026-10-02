# Load a PNG or BMP Image

Use <xref:Akeldov.Math.Spatial2D.Imaging.RasterImageLoader> to read an image file or a readable
stream into a new mutable `Raster<TColor>` owned by the caller. Choose the method that matches
the input pixel format; PNG loading does not convert between color types or channel depths.

## Choose the Pixel Type

| Method | Supported input | Result cell type |
| --- | --- | --- |
| `LoadGray8FromPng` | 8-bit grayscale PNG without alpha | `Gray8BitColor` |
| `LoadGray16FromPng` | 16-bit grayscale PNG without alpha | `Gray16BitColor` |
| `LoadRgba8FromPng` | PNG with 8 bits per RGBA channel | `RGBA8BitColor` |
| `LoadRgba16FromPng` | PNG with 16 bits per RGBA channel | `RGBA16BitColor` |
| `LoadGray8FromBmp` | Uncompressed indexed 8-bit BMP with a grayscale palette | `Gray8BitColor` |
| `LoadRgba8FromBmp` | Uncompressed indexed 8-bit BMP with any palette, or 32-bit BGRA BMP | `RGBA8BitColor` |

PNG loading supports all five scanline filters and Adam7 interlacing. It preserves channel
values and alpha without color-space conversion. Other PNG color formats and bit depths are
unsupported.

BMP loading supports Windows information headers, row padding, and both bottom-up and top-down
images. Palette indices are resolved to colors: indexed RGBA output is opaque, while grayscale
loading requires a grayscale palette. For 32-bit BGRA, the fourth byte is retained as alpha,
matching `SaveAsBmp`. Other BMP bit depths, compression methods, and OS/2 headers are unsupported.

## Load from a File or Stream

```csharp
using System.IO;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

Raster<Gray16BitColor> heightmap = RasterImageLoader.LoadGray16FromPng("heightmap.png");
Raster<Gray8BitColor> mask = RasterImageLoader.LoadGray8FromBmp("mask.bmp");

using Stream input = File.OpenRead("preview.png");
Raster<RGBA8BitColor> preview = RasterImageLoader.LoadRgba8FromPng(input);
preview.SaveAsPng("preview-copy.png");
```

Every method has file-path and stream overloads. Stream loading starts at the current position,
does not require seeking, and leaves the caller's stream open. PNG reading stops after the
`IEND` chunk; BMP reading consumes the file size declared in its header.

## Coordinates and Errors

The returned raster uses Y increasing from the bottom image row, matching the export methods.
It contains pixel resolution and values, without `RasterGeometry` world-space bounds. Retain
spatial metadata separately when an image represents a world-space field.

Invalid, truncated, or oversized image data throws `InvalidDataException`; unsupported formats
throw `NotSupportedException`. A null path or stream throws `ArgumentNullException`, and a
non-readable stream throws `ArgumentException`.

See [Imaging](../../concepts/imaging.md) for color types and image export, or
[Rasterization](index.md) to create an image from geometry.
