# RGBA Rasters

`RGBA8BitRaster` and `RGBA16BitRaster` store color samples with alpha.

Use RGBA rasters for heatmaps, culling maps, and visual diagnostics where a single grayscale channel is not enough.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGrid(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var color = new RGBA8BitColor(255, 128, 0, 255);
var values = new RGBA8BitColor[grid.Resolution.X * grid.Resolution.Y];
values[0] = color;

var raster = new RGBA8BitRaster(grid, values);
```
