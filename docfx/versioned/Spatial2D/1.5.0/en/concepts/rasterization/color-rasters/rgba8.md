# Raster&lt;RGBA8BitColor&gt;

Use `Raster<RGBA8BitColor>` for an image buffer and `SpatialRaster<RGBA8BitColor>`
for the same pixel format with world-space bounds. The cell type is <xref:Akeldov.Math.Spatial2D.Imaging.RGBA8BitColor>.
It stores `R`, `G`, `B`, and `A` in the integer range `0–255`.

Alpha zero is transparent; maximum alpha is opaque. RGB channels store color independently
of alpha (straight alpha).

Use it for ordinary color images and overlays. On `SpatialRaster<RGBA8BitColor>`, `Draw`,
`Fill`, `DrawArrow`, `DrawGrid`, and `DrawText` modify the existing buffer in world coordinates.
See [Draw Shapes and Text on a Raster](../../../how-to-guides/rasterization/draw-on-a-raster.md).

## Create, save, and load

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = RGBA8BitColor.FromNormalized(0.2f, 0.5f, 0.9f, 0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<RGBA8BitColor>(resolution, new[] { pixel, RGBA8BitColor.White });
image.SaveAsPng("rgba8.png");
Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromPng("rgba8.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<RGBA8BitColor>(grid, loaded.Values);
```

PNG export preserves 8-bit channels. BMP export and `LoadRgba8FromBmp` are also available.

## Operations and storage

`FromNormalized` clamps and rounds normalized channels. `Blend(from, to, amount)` interpolates
stored channels, including alpha; it is not source-over composition.

The constructors retain their arrays; `spatial` and `loaded` in the example share pixels.
Loading does not restore world bounds: the example supplies them explicitly. Clone `Values`
when independent storage is needed. Use `MapValues` for a new buffer with transformed colors.

See [Color Rasters](../color-rasters.md).
