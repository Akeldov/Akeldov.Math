# Project a Point onto a Curve

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var segment = new ParameterizedSegment(
    new PointXY(0f, 0f),
    new PointXY(10f, 0f));

var projection = segment.ProjectWithParameter(new PointXY(4f, 3f));

PointXY closestPoint = projection.ProjectedPoint;
float curveCoordinate = projection.CurveCoordinate;
float distance = projection.Distance;
```
