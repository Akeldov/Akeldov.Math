# SpatialBoolRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.SpatialBoolRaster> stores Boolean masks with world-space geometry.

## Operations

`!` inverts cells; `&`, `|`, and `^` combine corresponding cells. `==` and `!=` return
comparison masks, including comparisons with a scalar `bool`; they do not return one Boolean.
Use `is null` for null checks and `ReferenceEquals` for object identity.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(2f, 2f), new VectorXYInt(2, 2));
var raster = new SpatialBoolRaster(grid, new[] { true, false, true, false });
SpatialBoolRaster inverted = !raster;
SpatialBoolRaster selected = raster & inverted;
SpatialBoolRaster same = raster == true;
```

## Storage and compatibility

The constructor retains its array. `Values` and the indexers expose mutable state. Operators
allocate new mutable results owned by the caller and leave their operands unchanged.

Two spatial operands must have equal `Geometry`. Mixed operators with non-spatial rasters
require equal resolution and preserve the spatial operand's geometry. Inputs are not resampled.
The implicit conversion to `SpatialRaster<bool>` creates a wrapper sharing the same array;
changes through either wrapper are visible through the other. Keep spatial operand types when
geometry must be checked and preserved.

See [Spatial Rasters](../spatial-rasters.md).
