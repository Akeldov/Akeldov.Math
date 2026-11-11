# Raster&lt;Gray8BitColor&gt;

Use `Raster<Gray8BitColor>` for an image buffer and `SpatialRaster<Gray8BitColor>`
for the same pixel format with world-space bounds. The cell type is <xref:Akeldov.Math.Spatial2D.Imaging.Gray8BitColor>.
It stores `Value` in the integer range `0–255`.

Zero intensity is black and maximum intensity is white. There is no alpha channel; a black
cell is not automatically transparent.

Use it for compact masks and grayscale previews. Keep a separate Boolean raster if later work
needs logical mask operators.

## Create, save, and load

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = Gray8BitColor.FromNormalized(0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<Gray8BitColor>(resolution, new[] { pixel, Gray8BitColor.White });
image.SaveAsPng("gray8.png");
Raster<Gray8BitColor> loaded = RasterImageLoader.LoadGray8FromPng("gray8.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<Gray8BitColor>(grid, loaded.Values);
```

PNG export preserves 8-bit channels. BMP export and `LoadGray8FromBmp` are also available.

## Operations and storage

`FromNormalized` clamps and rounds normalized channels. `Blend(from, to, amount)` interpolates
stored channels.

The constructors retain their arrays; `spatial` and `loaded` in the example share pixels.
Loading does not restore world bounds: the example supplies them explicitly. Clone `Values`
when independent storage is needed. Use `MapValues` for a new buffer with transformed colors.

See [Color Rasters](../color-rasters.md).
