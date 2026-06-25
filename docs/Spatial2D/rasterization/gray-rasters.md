# Gray Rasters

`Raster<byte>` and `Raster<ushort>` store grayscale samples.

Use 8-bit rasters for masks and lightweight previews.
Use 16-bit rasters when signed-distance or smooth field output needs more precision.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGrid(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var values = new byte[grid.Resolution.X * grid.Resolution.Y];
var raster = new Raster<byte>(grid, values);
raster.SaveAsBmp("preview.bmp");
```
