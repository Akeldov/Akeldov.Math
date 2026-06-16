# Projections and Distances

`Project` returns the closest point on a curve and the distance from the sampled point to that curve.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var segment = new Segment(
    new PointXY(0f, 0f),
    new PointXY(10f, 0f));

CurveProjection projection = segment.Project(new PointXY(4f, 3f));

PointXY projectedPoint = projection.ProjectedPoint;
float distance = projection.Distance;
```

`ProjectWithParameter` also returns a curve coordinate. Curve coordinates are measured in world coordinate units along the parameterized curve.

```csharp
var path = new ParameterizedSegment(
    new PointXY(0f, 0f),
    new PointXY(10f, 0f));

ParameterizedCurveProjection pathProjection =
    path.ProjectWithParameter(new PointXY(4f, 3f));

float curveCoordinate = pathProjection.CurveCoordinate;
PointXY samePoint = path.GetPoint(curveCoordinate);
```

Infinite parameterized lines use signed coordinates. Rays start at `0`. Finite paths use `[0, Length]`.
