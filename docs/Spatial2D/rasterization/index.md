# Rasterization and Imaging

Rasterization samples geometry on a rectangular grid.

Spatial raster grids, rasters, rasterizers, and scene composition live in
the `Akeldov.Math.Spatial2D.Rasterization` namespace.
Image export helpers and color types live in `Akeldov.Math.Spatial2D.Imaging`.
Reusable spatial rasterization strategies implement `ISpatialRasterizer<TSource, TValue>`.
Reusable non-spatial rasterization strategies implement `IRasterizer<TSource, TValue>`.

`SpatialRaster<TValue>` stores values sampled on a `SpatialRasterGrid`.
`Raster<TValue>` stores the same rectangular value layout without world-space origin or size.
Both implement `IGrid<TValue>`, so image export works for either spatial or non-spatial rasters.

## Topics

- [SpatialRasterGrid](spatial-raster-grid.md)
- [Gray Rasters](gray-rasters.md)
- [RGBA Rasters](rgba-rasters.md)
- [GeometryScene](geometry-scenes.md)
- [Text Layers](text-layers.md)
- [Curve Distance Rasterization](curve-distance-rasterization.md)
- [Contour Signed Distance](contour-signed-distance.md)
- [Region Signed Distance](region-signed-distance.md)
- [Influence Field Heatmaps](influence-field-heatmaps.md)

## PNG and BMP Export

Image export helpers live in `Akeldov.Math.Spatial2D.Imaging`.

Use BMP export for simple 8-bit previews and PNG export for 16-bit grayscale or RGBA output.
PNG export accepts `IGrid<byte>`, `IGrid<ushort>`, `IGrid<RGBA8BitColor>`, and
`IGrid<RGBA16BitColor>`.
BMP export accepts `IGrid<byte>` and `IGrid<RGBA8BitColor>`.
Image files need raster dimensions and values, not world-space bounds.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new SpatialRasterGrid(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(64, 64));

var mask = new SpatialRaster<byte>(
    grid,
    new byte[grid.Resolution.X * grid.Resolution.Y]);
mask.SaveAsBmp("mask.bmp");

Raster<ushort> distance = new SpatialRaster<ushort>(
    grid,
    new ushort[grid.Resolution.X * grid.Resolution.Y])
    .ToRaster();
distance.SaveAsPng("distance.png");
```
