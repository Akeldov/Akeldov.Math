# SpatialRaster&lt;T&gt;

<xref:Akeldov.Math.Spatial2D.Rasterization.SpatialRaster`1> extends `Raster<T>` with `Geometry`
and implements `ISpatialRaster<T>`. Use it for values sampled in a known world-space rectangle.

## Create a spatial buffer

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(4f, 2f), new VectorXYInt(2, 2));
var raster = new SpatialRaster<float>(grid, new[] { 1f, 2f, 3f, 4f });
PointXY center = raster.Geometry.GetCellCenter(0, 0);
SpatialRaster<bool> mask = raster.MapValues(value => value >= 3f);
```

The constructor retains the supplied array. Origin and size must be finite, size and resolution
must be positive, and the array length must match the cell count. Inherited indexers mutate
the row-major `Values` buffer; `Geometry` remains fixed.

## Sampling and mapping

`grid.Rasterize(point => ...)` samples a function at every cell center. Field and geometry
rasterizers also return `SpatialRaster<T>`. `MapValues` preserves geometry and creates a new
mutable result owned by the caller when the input is typed as `ISpatialRaster<T>`.

`SpatialRaster<float>` stores numbers but does not itself add arithmetic operators. Use
[SpatialFloatRaster](spatial-float-raster.md), [SpatialIntRaster](spatial-int-raster.md), or
[SpatialBoolRaster](spatial-bool-raster.md) for operator support. Those specialized classes
implement `ISpatialRaster<T>` and offer implicit conversions to the generic spatial container;
the conversion creates a wrapper sharing the same array.

See [Spatial Rasters](../spatial-rasters.md) and [Color Rasters](../color-rasters.md).
