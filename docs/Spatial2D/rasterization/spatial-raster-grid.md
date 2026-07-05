# SpatialRasterGrid

`SpatialRasterGrid` maps integer raster cells to world-space sample points.
The grid origin is the lower-left corner, and each cell is sampled at its center.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new SpatialRasterGrid(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(160, 160));

PointXY center = grid.GetCellCenter(0, 0);
```

Use `VectorXYInt` for raster resolution and world-space `PointXY`/`VectorXY` values for bounds.
