# IntRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.IntRaster> stores integer counts and labels without world-space bounds.

## Operations

Supports cell-wise `+`, binary `-`, and unary `-`. Scalar addition and subtraction work
on either side. Raster operands can be specialized or matching generic containers.

Integer arithmetic uses unchecked overflow semantics; values wrap rather than saturate.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var resolution = new VectorXYInt(2, 2);
var raster = new IntRaster(resolution, new[] { 1, 2, 3, 4 });
IntRaster shifted = raster + 2;
IntRaster difference = shifted - raster;
IntRaster inverted = -raster;
```

## Storage and compatibility

The constructor retains its array. `Values` and the indexers expose mutable state. Operators
allocate new mutable results owned by the caller and leave their operands unchanged.

Binary raster operands must have equal resolution; there is no automatic resizing.

See [Non-Spatial Rasters](../non-spatial-rasters.md).
