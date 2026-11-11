# BoolRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.BoolRaster> stores Boolean masks without world-space bounds.

## Operations

`!` inverts cells; `&`, `|`, and `^` combine corresponding cells. `==` and `!=` return
comparison masks, including comparisons with a scalar `bool`; they do not return one Boolean.
Use `is null` for null checks and `ReferenceEquals` for object identity.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var resolution = new VectorXYInt(2, 2);
var raster = new BoolRaster(resolution, new[] { true, false, true, false });
BoolRaster inverted = !raster;
BoolRaster selected = raster & inverted;
BoolRaster same = raster == true;
```

## Storage and compatibility

The constructor retains its array. `Values` and the indexers expose mutable state. Operators
allocate new mutable results owned by the caller and leave their operands unchanged.

Binary raster operands must have equal resolution; there is no automatic resizing.

See [Non-Spatial Rasters](../non-spatial-rasters.md).
