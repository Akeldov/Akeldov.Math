# Gray Rasters

`Raster<Gray8BitColor>` and `Raster<Gray16BitColor>` store rectangular grayscale samples without spatial bounds.
`SpatialRaster<Gray8BitColor>` and `SpatialRaster<Gray16BitColor>` store grayscale samples together with a
`RasterGeometry`.

Use 8-bit rasters for masks and lightweight previews.
Use 16-bit rasters when signed-distance or smooth field output needs more precision.
Both raster types implement `IGrid<TValue>`, so `SaveAsBmp` and `SaveAsPng` can export either one.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO.Compression;

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var values = new Gray8BitColor[grid.Resolution.X * grid.Resolution.Y];
var raster = new SpatialRaster<Gray8BitColor>(grid, values);
raster.SaveAsBmp("preview.bmp");

Raster<Gray8BitColor> detachedRaster = raster.ToRaster();
detachedRaster.SaveAsPng("preview-copy.png", CompressionLevel.Fastest);
```

`ToRaster()` drops the spatial grid and returns a new mutable `Raster<TValue>` with a copied value
array owned by the caller.

`SaveAsPng` uses `CompressionLevel.Optimal` by default. Pass `NoCompression`, `Fastest`, `Optimal`,
or, on .NET 6 and later, `SmallestSize` when file size and encoding time need an explicit tradeoff.
