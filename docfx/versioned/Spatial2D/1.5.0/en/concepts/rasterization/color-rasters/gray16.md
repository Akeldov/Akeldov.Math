# Raster&lt;Gray16BitColor&gt;

Use `Raster<Gray16BitColor>` for an image buffer and `SpatialRaster<Gray16BitColor>`
for the same pixel format with world-space bounds. The cell type is <xref:Akeldov.Math.Spatial2D.Imaging.Gray16BitColor>.
It stores `Value` in the integer range `0–65535`.

Zero intensity is black and maximum intensity is white. There is no alpha channel; a black
cell is not automatically transparent.

Use it for height-map images and smooth scalar gradients. The extra precision reduces quantization,
but encoding a physical value still requires an explicit range and can clip information.

## Create, save, and load

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = Gray16BitColor.FromNormalized(0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<Gray16BitColor>(resolution, new[] { pixel, Gray16BitColor.White });
image.SaveAsPng("gray16.png");
Raster<Gray16BitColor> loaded = RasterImageLoader.LoadGray16FromPng("gray16.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<Gray16BitColor>(grid, loaded.Values);
```

PNG export preserves 16-bit channels. There is no direct BMP overload for this format; map to an 8-bit color type first.

## Operations and storage

`FromNormalized` clamps and rounds normalized channels. `Blend(from, to, amount)` interpolates
stored channels.

The constructors retain their arrays; `spatial` and `loaded` in the example share pixels.
Loading does not restore world bounds: the example supplies them explicitly. Clone `Values`
when independent storage is needed. Use `MapValues` for a new buffer with transformed colors.

See [Color Rasters](../color-rasters.md).
