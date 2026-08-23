# Intersections

`GetPointIntersections` returns isolated intersection points in the forward direction of a ray.

The returned collection is a new mutable list owned by the caller.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var rayCaster = new Ray(new PointXY(-10f, 0f), angle: 0f);
List<PointXY> hits = circle.GetPointIntersections(rayCaster);
```

Points that belong to a continuous set of intersections are not returned. For example, a
collinear overlap between a linear curve and the ray does not produce a representative point.

`GetPointIntersections` uses the library's standard geometry tolerance.
