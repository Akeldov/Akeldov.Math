# Raster&lt;RGBA16BitColor&gt;

Use `Raster<RGBA16BitColor>` for an image buffer and `SpatialRaster<RGBA16BitColor>`
for the same pixel format with world-space bounds. The cell type is <xref:Akeldov.Math.Spatial2D.Imaging.RGBA16BitColor>.
It stores `R`, `G`, `B`, and `A` in the integer range `0–65535`.

Alpha zero is transparent; maximum alpha is opaque. RGB channels store color independently
of alpha (straight alpha).

Use it for fine color gradients and layered scenes. `RGBA16BitColor.AlphaOver(background, foreground)`
composites the foreground over the background; `ScaleAlpha(coverage)` scales opacity.
`GeometryScene<RGBA16BitColor>` can use `AlphaOver` as its blend function.

## Create, save, and load

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = RGBA16BitColor.FromNormalized(0.2f, 0.5f, 0.9f, 0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<RGBA16BitColor>(resolution, new[] { pixel, RGBA16BitColor.White });
image.SaveAsPng("rgba16.png");
Raster<RGBA16BitColor> loaded = RasterImageLoader.LoadRgba16FromPng("rgba16.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<RGBA16BitColor>(grid, loaded.Values);
```

PNG export preserves 16-bit channels. There is no direct BMP overload for this format; map to an 8-bit color type first.

## Operations and storage

`FromNormalized` clamps and rounds normalized channels. `Blend(from, to, amount)` interpolates
stored channels, including alpha; it is not source-over composition.

The constructors retain their arrays; `spatial` and `loaded` in the example share pixels.
Loading does not restore world bounds: the example supplies them explicitly. Clone `Values`
when independent storage is needed. Use `MapValues` for a new buffer with transformed colors.

See [Color Rasters](../color-rasters.md).
