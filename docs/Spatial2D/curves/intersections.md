# Intersections

`ICurve.GetPointIntersections(Ray)` returns isolated intersection points in the forward direction of a ray.
Linear and circular curves also provide `GetPointIntersections(Line)` for intersections with an infinite line.

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

```csharp
var segment = new Segment(
    new PointXY(-2f, 1f),
    new PointXY(2f, 1f));

var probeLine = new Line(
    new PointXY(0f, -2f),
    new PointXY(0f, 2f));

List<PointXY> lineHits = segment.GetPointIntersections(probeLine);
```

Points that belong to a continuous set of intersections are not returned. For example, a
collinear overlap between a linear curve and the ray does not produce a representative point.

`GetPointIntersections(Ray)` uses the library's standard geometry tolerance. Linear- and
circular-curve intersections with `Line` use exact comparisons.
