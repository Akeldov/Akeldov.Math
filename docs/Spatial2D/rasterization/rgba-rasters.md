# RGBA Rasters

`Raster<RGBA8BitColor>` and `Raster<RGBA16BitColor>` store color samples with alpha.

Use RGBA rasters for heatmaps, culling maps, and visual diagnostics where a single grayscale channel is not enough.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGrid(
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

var raster = new Raster<RGBA8BitColor>(grid, values);
```

`RGBA8BitColor` and `RGBA16BitColor` both provide `FromNormalized` for channel values in the 0 to 1 range and `Blend` for linear color interpolation.
