# Intersections

`GetRayIntersections` returns intersection points in the forward direction of a ray.

The returned collection is a new mutable list owned by the caller.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var rayCaster = new Ray(new PointXY(-10f, 0f), angle: 0f);
List<PointXY> hits = circle.GetRayIntersections(rayCaster);
```

The `geometryEpsilon` argument is measured in world coordinate units and controls geometric comparisons near tangencies, collinear overlaps, nearly parallel lines, and endpoints.

For collinear overlaps, linear curves return the first point encountered by the ray rather than a segment of overlap.
