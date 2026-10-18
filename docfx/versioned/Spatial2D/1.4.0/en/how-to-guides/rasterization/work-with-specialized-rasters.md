# Work with Numeric and Boolean Rasters

Use the specialized raster types when you need cell-wise operators. They retain the same
row-major storage and mutable indexers as `Raster<T>`; X advances first in the backing array.

## Add and Subtract Numeric Values

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

var resolution = new VectorXYInt(2, 2);
var heights = new FloatRaster(resolution, new[] { 1f, 2f, 3f, 4f });
var offsets = new Raster<float>(resolution, new[] { 0.5f, 1f, 1.5f, 2f });
FloatRaster adjusted = heights + offsets - 0.5f;
FloatRaster inverted = -heights;

var counts = new IntRaster(resolution, new[] { 1, 2, 3, 4 });
IntRaster remaining = 10 - counts;
```

`FloatRaster` and `IntRaster` support `+`, binary `-`, and unary `-`. Scalar operands work on
either side; mixed raster operands must use the matching value type and resolution. Each operation
allocates a new mutable raster owned by the caller, leaving its inputs unchanged. Integer arithmetic
uses unchecked overflow semantics.

## Combine Boolean Masks

```csharp
var visible = new BoolRaster(resolution, new[] { true, false, true, false });
var blocked = new Raster<bool>(resolution, new[] { false, false, true, true });
BoolRaster available = visible & (visible != blocked);
BoolRaster hidden = !visible;
BoolRaster same = visible == blocked;
```

Boolean rasters support `!`, `&`, `|`, `^`, `==`, and `!=`. Equality and inequality compare each
cell and return another mask; they do not return a single Boolean or compare raster references.
Use `raster is null` to test null and `ReferenceEquals` to compare objects. Scalar Boolean
comparisons such as `visible == true` also produce masks.

## Retain Geometry and Share a Generic View

```csharp
var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 2f), resolution);
var spatialHeights = new SpatialFloatRaster(grid, new[] { 1f, 2f, 3f, 4f });
SpatialFloatRaster shifted = spatialHeights + offsets;
var spatialCounts = new SpatialIntRaster(grid, new[] { 1, 2, 3, 4 });
SpatialIntRaster increased = spatialCounts + counts;
var spatialMask = new SpatialBoolRaster(grid, new[] { true, true, false, false });
SpatialBoolRaster selected = spatialMask & visible;

SpatialRaster<float> shared = spatialHeights;
shared[0, 0] = 10f;
Console.WriteLine(spatialHeights[0, 0]); // 10
```

Operations between spatial operands require equal `RasterGeometry`, including origin, size,
and resolution. Overloads with non-spatial operands check resolution and retain the spatial
operand's geometry. They do not resample either input.

The implicit conversions from specialized spatial types to `SpatialRaster<T>` allocate a new
wrapper and share the same value array. Constructors also retain the supplied array. Clone the
array when you need independent storage; `Values` is retained mutable state, not a copy.
Keep operands typed as spatial rasters when their geometry must be checked and preserved:
upcasting to `Raster<T>` selects the non-spatial overloads.

## Rasterize Distances into Values

```csharp
var segment = new Segment(new PointXY(0f, 0f), new PointXY(2f, 0f));
SpatialRaster<float> distances = segment.Rasterize(distance => distance, grid);
SpatialRaster<bool> nearSegment = segment.Rasterize(distance => distance < 1f, grid);
var distanceValues = new SpatialFloatRaster(grid, distances.Values);
SpatialFloatRaster offsetDistances = distanceValues - 0.25f;
```

The generic `Rasterize<T, TResult>` overload samples cell centers and maps world-space distances
to the selected value type. When the declared source type implements `ISignedPointDistanceProvider`,
the selector receives negative distances inside and positive distances outside; otherwise it receives
unsigned distances. The example uses an unsigned segment distance. Rasterization allocates a new
mutable result owned by the caller; `distanceValues` then shares its array with `distances`.
