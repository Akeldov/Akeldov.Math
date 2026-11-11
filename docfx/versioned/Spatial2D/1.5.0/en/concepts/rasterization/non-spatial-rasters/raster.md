# Raster&lt;T&gt;

<xref:Akeldov.Math.Spatial2D.Rasterization.Raster`1> stores arbitrary cell values without world bounds.
Use it when indices and resolution are sufficient. It implements `IRaster<T>` and is the base
of `BoolRaster`, `IntRaster`, `FloatRaster`, and `SpatialRaster<T>`.

## Create and access values

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var raster = new Raster<int>(new VectorXYInt(2, 2), new[] { 1, 2, 3, 4 });
raster[1, 0] = 10;
Raster<bool> mask = raster.MapValues(value => value >= 3);
```

The constructor retains the array. All three indexers (`[x, y]`, `[VectorXYInt]`, and flat
`[index]`) read and write the same row-major buffer exposed by `Values`. Clone the array before
construction when the raster needs independent storage. Invalid indices throw exceptions.

## Transform values

`MapValues(value => ...)` creates a new mutable `Raster<TResult>` owned by the caller without
changing resolution. The generic container has no numeric or Boolean operator overloads; use
the matching specialized raster when operators are needed.

See [Non-Spatial Rasters](../non-spatial-rasters.md) and [SpatialRaster<T>](../spatial-rasters/spatial-raster.md).
