# SpatialIntRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.SpatialIntRaster> stores integer counts and labels with world-space geometry.

## Operations

Supports cell-wise `+`, binary `-`, and unary `-`. Scalar addition and subtraction work
on either side. Raster operands can be specialized or matching generic containers.

Integer arithmetic uses unchecked overflow semantics; values wrap rather than saturate.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(2f, 2f), new VectorXYInt(2, 2));
var raster = new SpatialIntRaster(grid, new[] { 1, 2, 3, 4 });
SpatialIntRaster shifted = raster + 2;
SpatialIntRaster difference = shifted - raster;
SpatialIntRaster inverted = -raster;
```

## Storage and compatibility

The constructor retains its array. `Values` and the indexers expose mutable state. Operators
allocate new mutable results owned by the caller and leave their operands unchanged.

Two spatial operands must have equal `Geometry`. Mixed operators with non-spatial rasters
require equal resolution and preserve the spatial operand's geometry. Inputs are not resampled.
The implicit conversion to `SpatialRaster<int>` creates a wrapper sharing the same array;
changes through either wrapper are visible through the other. Keep spatial operand types when
geometry must be checked and preserved.

See [Spatial Rasters](../spatial-rasters.md).
