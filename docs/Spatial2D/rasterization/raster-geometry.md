# RasterGeometry

`RasterGeometry` maps integer raster cells to world-space sample points.
The grid origin is the lower-left corner, and each cell is sampled at its center.
Use it when rasterization needs a world-space region.
Use `Raster<TValue>` when only a rectangular value grid is needed and origin/size are not part of
the data.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(160, 160));

PointXY center = grid.GetCellCenter(0, 0);
```

Use `VectorXYInt` for raster resolution and world-space `PointXY`/`VectorXY` values for bounds.

## Perlin noise

`CreatePerlinNoise` samples deterministic fractal Perlin noise at every raster cell center and
returns a mutable `SpatialRaster<float>` with values in the `[0, 1]` range.

```csharp
SpatialRaster<float> heights = grid.CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5,
    persistence: 0.5f,
    lacunarity: 2f);
```

`scale` and `offset` use world-coordinate units. Adjacent raster geometries therefore sample
matching parts of the same noise field when their world-space bounds are aligned.

When spatial bounds are not needed, call the same extension on a resolution to create a
non-spatial unit-cell raster:

```csharp
Raster<float> heights = new VectorXYInt(128, 96).CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5);
```

This overload samples cell centers at `(x + 0.5, y + 0.5)`, and interprets `scale` and `offset`
in cell units. Its values match a `RasterGeometry` with zero origin, unit cells, and the same
resolution.
