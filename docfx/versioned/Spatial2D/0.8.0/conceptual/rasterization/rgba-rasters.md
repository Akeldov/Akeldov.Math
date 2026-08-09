# RGBA Rasters

`Raster<RGBA8BitColor>` and `Raster<RGBA16BitColor>` store rectangular color samples with alpha.
`SpatialRaster<RGBA8BitColor>` and `SpatialRaster<RGBA16BitColor>` store the same color values
together with a `RasterGeometry`.

Use RGBA rasters for heatmaps, culling maps, and visual diagnostics where a single grayscale channel is not enough.
Both raster types implement `IGrid<TValue>`, so PNG export works for spatial and non-spatial RGBA
rasters.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var color = RGBA8BitColor.FromNormalized(1f, 0.5f, 0f);
var darkerColor = RGBA8BitColor.Blend(
    color,
    RGBA8BitColor.FromNormalized(0f, 0f, 0f),
    0.25f);
var values = new RGBA8BitColor[grid.Resolution.X * grid.Resolution.Y];
values[0] = darkerColor;

var raster = new SpatialRaster<RGBA8BitColor>(grid, values);
raster.SaveAsPng("colors.png");

Raster<RGBA8BitColor> detachedRaster = raster.ToRaster();
detachedRaster.SaveAsPng("colors-copy.png");
```

`RGBA8BitColor` and `RGBA16BitColor` both provide `FromNormalized` for channel values in the 0 to 1 range and `Blend` for linear color interpolation.
