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

Use the static `LoadFromPng` overloads on `Raster<RGBA8BitColor>`, `Raster<RGBA16BitColor>`, or `Raster<Gray16BitColor>`
to load a file or a stream:

```csharp
using System.IO;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

Raster<RGBA16BitColor> fromFile = Raster<RGBA16BitColor>.LoadFromPng("scene.png");

using Stream stream = File.OpenRead("scene.png");
Raster<RGBA16BitColor> fromStream = Raster<RGBA16BitColor>.LoadFromPng(stream);

Raster<RGBA8BitColor> rgba8FromFile = Raster<RGBA8BitColor>.LoadFromPng("preview.png");
using Stream rgba8Stream = File.OpenRead("preview.png");
Raster<RGBA8BitColor> rgba8FromStream = Raster<RGBA8BitColor>.LoadFromPng(rgba8Stream);

Raster<Gray16BitColor> gray16FromFile = Raster<Gray16BitColor>.LoadFromPng("heightmap.png");
using Stream gray16Stream = File.OpenRead("heightmap.png");
Raster<Gray16BitColor> gray16FromStream = Raster<Gray16BitColor>.LoadFromPng(gray16Stream);
```

The PNG color type and bit depth must match the requested raster type:
8-bit RGBA for `RGBA8BitColor`, 16-bit RGBA for `RGBA16BitColor`,
or 16-bit grayscale without alpha for `Gray16BitColor`.
Loading preserves channel values and alpha without
color-space conversion, and supports all five scanline filters and Adam7 interlacing.
The loaded raster is new, mutable, and owned by the caller. Y increases from the bottom image
row, matching `SaveAsPng`. PNG does not retain `RasterGeometry` spatial bounds.

Stream loading starts at the current position, stops after the PNG trailer, and leaves the
stream open. Seeking is not required. Invalid or truncated PNG data throws `InvalidDataException`;
unsupported PNG color formats or raster value types throw `NotSupportedException`.
