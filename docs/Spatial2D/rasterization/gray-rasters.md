# Gray Rasters

`Raster<byte>` and `Raster<ushort>` store rectangular grayscale samples without spatial bounds.
`SpatialRaster<byte>` and `SpatialRaster<ushort>` store grayscale samples together with a
`SpatialRasterGrid`.

Use 8-bit rasters for masks and lightweight previews.
Use 16-bit rasters when signed-distance or smooth field output needs more precision.
Both raster types implement `IGrid<TValue>`, so `SaveAsBmp` and `SaveAsPng` can export either one.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new SpatialRasterGrid(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var values = new byte[grid.Resolution.X * grid.Resolution.Y];
var raster = new SpatialRaster<byte>(grid, values);
raster.SaveAsBmp("preview.bmp");

Raster<byte> detachedRaster = raster.ToRaster();
detachedRaster.SaveAsPng("preview-copy.png");
```

`ToRaster()` drops the spatial grid and returns a new mutable `Raster<TValue>` with a copied value
array owned by the caller.
