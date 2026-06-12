# Rasterization and Imaging

Rasterization samples geometry on a rectangular grid.

The core types live in the `Akeldov.Math.Spatial2D.Rasterization` namespace.
Rasters and image export helpers live in `Akeldov.Math.Spatial2D.Imaging`.

## Topics

- [RasterGrid](raster-grid.md)
- [Gray Rasters](gray-rasters.md)
- [RGBA Rasters](rgba-rasters.md)
- [Curve Distance Rasterization](curve-distance-rasterization.md)
- [Contour Signed Distance](contour-signed-distance.md)
- [Region Signed Distance](region-signed-distance.md)
- [Influence Field Heatmaps](influence-field-heatmaps.md)

## PNG and BMP Export

Raster image helpers live in `Akeldov.Math.Spatial2D.Imaging`.

Use BMP export for simple 8-bit previews and PNG export for 16-bit grayscale or RGBA output.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGrid(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var mask = new Gray8BitRaster(
    grid,
    new byte[grid.Resolution.X * grid.Resolution.Y]);
mask.SaveAsBmp("mask.bmp");

var distance = new Gray16BitRaster(
    grid,
    new ushort[grid.Resolution.X * grid.Resolution.Y]);
distance.SaveAsPng("distance.png");
```
