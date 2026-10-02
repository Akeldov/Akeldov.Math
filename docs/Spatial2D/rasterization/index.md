# Rasterization and Imaging

Rasterization samples geometry on a rectangular grid.

Raster geometry, rasters, rasterizers, and scene composition live in
the `Akeldov.Math.Spatial2D.Rasterization` namespace.
Image export helpers and color types live in `Akeldov.Math.Spatial2D.Imaging`.
Reusable spatial rasterization strategies implement `ISpatialRasterizer<TSource, TValue>`.
Reusable non-spatial rasterization strategies implement `IRasterizer<TSource, TValue>`.

## Raster Types

The library has two raster contracts and corresponding implementations:

| Contract | Implementation | Spatial information |
| --- | --- | --- |
| `IRaster<TValue>` | `Raster<TValue>` | Resolution and row-major values only. |
| `ISpatialRaster<TValue>` | `SpatialRaster<TValue>` | Adds `RasterGeometry`, which contains the world-space origin, size, and resolution. |

`ISpatialRaster<TValue>` extends `IRaster<TValue>`, and `SpatialRaster<TValue>` derives from `Raster<TValue>`. A spatial raster can therefore be passed to APIs that only require an ordinary raster. Use `ToRaster()` when a new non-spatial raster with a copied value array is required.

Raster dimensions must be positive, their product must fit in a one-dimensional array, and the retained row-major value array must contain exactly one value per cell. `SpatialRaster<TValue>` additionally requires a valid, non-default `RasterGeometry`.

## Field Rasterization

Any `IField<TValue>` can be sampled at raster cell centers and mapped to a raster value type with `Rasterize`:

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

SpatialRaster<Gray8BitColor> raster = field.Rasterize(
    grid,
    value => value ? Gray8BitColor.White : Gray8BitColor.Black);
```

Cells are sampled in row-major order, starting with the lower-left row. The returned `SpatialRaster<TValue>` has a new mutable value array owned by the caller.

## Color Types

Four color value types are available in `Akeldov.Math.Spatial2D.Imaging`:

| Type | Channels | Precision | Total size |
| --- | --- | --- | --- |
| `Gray8BitColor` | Grayscale | 8 bits | 8 bits per pixel |
| `Gray16BitColor` | Grayscale | 16 bits | 16 bits per pixel |
| `RGBA8BitColor` | Red, green, blue, alpha | 8 bits per channel | 32 bits per pixel |
| `RGBA16BitColor` | Red, green, blue, alpha | 16 bits per channel | 64 bits per pixel |

The grayscale types represent intensity without alpha. The RGBA types include an alpha channel for transparency and compositing. All four can be used as `Raster<TValue>` or `SpatialRaster<TValue>` values.

## Topics

- [RasterGeometry](raster-geometry.md)
- [Gray Rasters](gray-rasters.md)
- [RGBA Rasters](rgba-rasters.md)
- [GeometryScene](geometry-scenes.md)
- [Text Layers](text-layers.md)
- [Curve Distance Rasterization](curve-distance-rasterization.md)
- [Contour Signed Distance](contour-signed-distance.md)
- [Region Signed Distance](region-signed-distance.md)
- [Influence Field Heatmaps](influence-field-heatmaps.md)

## PNG and BMP Export

Image export helpers live in `Akeldov.Math.Spatial2D.Imaging`.

Use BMP export for simple 8-bit previews and PNG export for 16-bit grayscale or RGBA output.
PNG export accepts `IRaster<Gray8BitColor>`, `IRaster<Gray16BitColor>`,
`IRaster<RGBA8BitColor>`, and `IRaster<RGBA16BitColor>`.
BMP export accepts `IRaster<Gray8BitColor>` and `IRaster<RGBA8BitColor>`.
Image files need raster dimensions and values, not world-space bounds.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var mask = new SpatialRaster<Gray8BitColor>(
    grid,
    new Gray8BitColor[grid.Resolution.X * grid.Resolution.Y]);
mask.SaveAsBmp("mask.bmp");

Raster<Gray16BitColor> distance = new SpatialRaster<Gray16BitColor>(
    grid,
    new Gray16BitColor[grid.Resolution.X * grid.Resolution.Y])
    .ToRaster();
distance.SaveAsPng("distance.png");
```

## Reading PNG

Use `RasterImageLoader` in `Akeldov.Math.Spatial2D.Imaging` to load a file or a stream.
Choose `LoadRgba8FromPng`, `LoadRgba16FromPng`, `LoadGray8FromPng`, or `LoadGray16FromPng`
for the required pixel type:

```csharp
using System.IO;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

Raster<RGBA16BitColor> fromFile = RasterImageLoader.LoadRgba16FromPng("scene.png");

using Stream stream = File.OpenRead("scene.png");
Raster<RGBA16BitColor> fromStream = RasterImageLoader.LoadRgba16FromPng(stream);

Raster<RGBA8BitColor> rgba8FromFile = RasterImageLoader.LoadRgba8FromPng("preview.png");
using Stream rgba8Stream = File.OpenRead("preview.png");
Raster<RGBA8BitColor> rgba8FromStream = RasterImageLoader.LoadRgba8FromPng(rgba8Stream);

Raster<Gray16BitColor> gray16FromFile = RasterImageLoader.LoadGray16FromPng("heightmap.png");
using Stream gray16Stream = File.OpenRead("heightmap.png");
Raster<Gray16BitColor> gray16FromStream = RasterImageLoader.LoadGray16FromPng(gray16Stream);

Raster<Gray8BitColor> gray8FromFile = RasterImageLoader.LoadGray8FromPng("mask.png");
using Stream gray8Stream = File.OpenRead("mask.png");
Raster<Gray8BitColor> gray8FromStream = RasterImageLoader.LoadGray8FromPng(gray8Stream);
```

The PNG color type and bit depth must match the requested raster type:
8-bit RGBA for `RGBA8BitColor`, 16-bit RGBA for `RGBA16BitColor`,
8-bit grayscale without alpha for `Gray8BitColor`,
or 16-bit grayscale without alpha for `Gray16BitColor`.
Loading preserves channel values and alpha without
color-space conversion, and supports all five scanline filters and Adam7 interlacing.
The loaded raster is new, mutable, and owned by the caller. Y increases from the bottom image
row, matching `SaveAsPng`. PNG does not retain `RasterGeometry` spatial bounds.

Stream loading starts at the current position, stops after the PNG trailer, and leaves the
stream open. Seeking is not required. Invalid or truncated PNG data throws `InvalidDataException`;
unsupported PNG color formats or bit depths throw `NotSupportedException`.

## Reading BMP

Use `RasterImageLoader.LoadGray8FromBmp` or `RasterImageLoader.LoadRgba8FromBmp`.
Each method has overloads for a file path and a stream:

```csharp
Raster<Gray8BitColor> mask = RasterImageLoader.LoadGray8FromBmp("mask.bmp");
using Stream bmpStream = File.OpenRead("preview.bmp");
Raster<RGBA8BitColor> preview = RasterImageLoader.LoadRgba8FromBmp(bmpStream);
```

Both types support uncompressed indexed 8-bit BMP. Palette indices are resolved to colors;
`Gray8BitColor` requires a grayscale palette, while `RGBA8BitColor` accepts any palette and
returns opaque palette colors. `RGBA8BitColor` also reads uncompressed 32-bit BGRA BMP and
preserves the fourth pixel byte as alpha, matching `SaveAsBmp` output.
Other bit depths, compression methods, and OS/2 headers are unsupported.

Loading handles row padding and both bottom-up and top-down images. The returned raster is
new, mutable, and owned by the caller, with Y increasing from the bottom image row.
Spatial bounds are not stored in BMP. Stream loading starts at the current position, consumes
the declared BMP file size, and leaves the stream open. Seeking is not required.
Invalid or truncated data throws `InvalidDataException`; unsupported BMP formats throw
`NotSupportedException`.
