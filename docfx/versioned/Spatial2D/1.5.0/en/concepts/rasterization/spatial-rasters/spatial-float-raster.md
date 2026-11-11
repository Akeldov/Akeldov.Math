# SpatialFloatRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.SpatialFloatRaster> stores floating-point fields and distances with world-space geometry.

## Operations

Supports cell-wise `+`, binary `-`, and unary `-`. Scalar addition and subtraction work
on either side. Raster operands can be specialized or matching generic containers.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(2f, 2f), new VectorXYInt(2, 2));
var raster = new SpatialFloatRaster(grid, new[] { 1f, 2f, 3f, 4f });
SpatialFloatRaster shifted = raster + 0.5f;
SpatialFloatRaster difference = shifted - raster;
SpatialFloatRaster inverted = -raster;
```

## Storage and compatibility

The constructor retains its array. `Values` and the indexers expose mutable state. Operators
allocate new mutable results owned by the caller and leave their operands unchanged.

Two spatial operands must have equal `Geometry`. Mixed operators with non-spatial rasters
require equal resolution and preserve the spatial operand's geometry. Inputs are not resampled.
The implicit conversion to `SpatialRaster<float>` creates a wrapper sharing the same array;
changes through either wrapper are visible through the other. Keep spatial operand types when
geometry must be checked and preserved.

## Filtering and generation

`GaussianBlur(sigma)` or `GaussianBlur(sigma, radius)` returns a new `SpatialRaster<float>`.
Sigma and radius are measured in cells; edge weights are normalized over available cells.
`grid.CreatePerlinNoise(seed: 42, scale: 10f)` generates a new `SpatialRaster<float>`
with values in `[0, 1]`. These methods return generic containers; wrap their `Values` in
`SpatialFloatRaster` when arithmetic operators are needed, remembering that the wrapper shares storage.

Noise scale is measured in world units.

See [Spatial Rasters](../spatial-rasters.md).
