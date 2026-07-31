# Coordinate System

Akeldov.Math.Spatial2D uses a two-dimensional Cartesian coordinate system. Positions,
directions, sizes, and discrete values all have `X` and `Y` components, while their types
preserve the meaning of those components.

## Cartesian axes

The positive X axis points to the right and the positive Y axis points upward:

```csharp
using Akeldov.Math.Spatial2D;

VectorXY right = VectorXY.BasisX; // (1, 0)
VectorXY up = VectorXY.BasisY;    // (0, 1)
```

For rotation direction and measurement conventions, see
[Angles and Units](../angles-and-units.md).

## Choose the value by meaning

| Type | Meaning | Typical uses |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.PointXY> | A position in continuous space | Endpoints, centers, samples, intersections |
| <xref:Akeldov.Math.Spatial2D.VectorXY> | A floating-point vector | Directions, offsets, sizes, transformations |
| <xref:Akeldov.Math.Spatial2D.VectorXYInt> | An integer vector | Integer directions, offsets, sizes, indices, resolutions |

`PointXY` represents a position, while `VectorXY` and `VectorXYInt` represent vectors.
`VectorXYInt` is not limited to indexing; some APIs also use it as a two-dimensional index or
resolution.

`PointXY` and `VectorXY` use `float` components. Conversion to `VectorXYInt` can discard a
fractional part, so the [Vectors](vectors.md) page describes the available conversion rules.

Raster APIs connect continuous and discrete values explicitly. For example,
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> combines a `PointXY` origin and
`VectorXY` size with a `VectorXYInt` resolution. See [Discrete Indices](discrete-indices.md) for
raster addressing and world-space mapping.

## Topics

- [Points](points.md) — positions, distance, interpolation, and point transformations.
- [Vectors](vectors.md) — floating-point and integer vectors, direction operations, and
  conversion.
- [Discrete Indices](discrete-indices.md) — raster indices, resolutions, offsets, and
  world-space mapping.
- [Angles and Units](../angles-and-units.md) — rotation, angular representation, and measurement
  conventions.
