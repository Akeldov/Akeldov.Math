# Project a Point onto a Curve

Use `Project` to find the closest point on any `ICurve` and the distance to it.

## Project onto any curve

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

ICurve curve = new Segment(
    new PointXY(0f, 0f),
    new PointXY(10f, 0f));

var sample = new PointXY(4f, 3f);
CurveProjection projection = curve.Project(sample);

PointXY closestPoint = projection.ProjectedPoint; // (4, 0)
float distance = projection.Distance;              // 3
```

Call `curve.Distance(sample)` instead when only the distance is needed.

## Preserve the curve coordinate

For an `IParameterizedCurve`, use `ProjectWithParameter` to also obtain the projected position's
coordinate along the curve:

```csharp
var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

ParameterizedCurveProjection pathProjection =
    path.ProjectWithParameter(sample);

float curveCoordinate = pathProjection.CurveCoordinate; // 4
PointXY samePoint = path.GetPoint(curveCoordinate);       // (4, 0)
```

Curve coordinates use world coordinate units, not a normalized `[0, 1]` range. On this segment,
the coordinate is the distance from `StartPoint` and lies in `[0, Length]`.

Bounded segments and arcs project to the nearest endpoint when the unconstrained closest point
lies beyond their extent. The input point must have finite coordinates.

For more background, see [Curves](../../concepts/geometry-model/curves.md). Next, learn how to
[find curve intersections](find-curve-intersections.md).
