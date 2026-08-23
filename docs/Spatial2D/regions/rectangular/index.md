# Rectangular Regions

Rectangular regions represent filled rectangles, either aligned with the world axes or rotated in world space.

| Region | Orientation | Construction |
|---|---|---|
| [`Rectangle`](../rectangle.md) | Axis-aligned | Two opposite corners |
| [`OrientedRectangle`](../oriented-rectangle.md) | Arbitrary | Center, size, and rotation in radians |

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var axisAligned = new Rectangle(
    cornerA: new PointXY(0f, 0f),
    cornerB: new PointXY(10f, 6f));

var rotated = new OrientedRectangle(
    center: new PointXY(0f, 0f),
    size: new VectorXY(8f, 3f),
    rotation: MathF.PI / 6f);
```

Both types provide `Contains`, `Distance`, and `SignedDistance`. Choose `Rectangle` when the sides follow the coordinate axes and `OrientedRectangle` when the region has its own rotated local frame.

Rectangle dimensions may be zero but never negative. One zero dimension produces a segment and two
zero dimensions produce a point; positive width and height produce an area. Both region structures
therefore have valid `default` values representing the origin point. For degenerate regions,
`SignedDistance` is zero on the represented set and positive outside it.

These rules apply to rectangular geometry values, not to raster grids. `RasterGeometry` retains its
own size and resolution validation.
