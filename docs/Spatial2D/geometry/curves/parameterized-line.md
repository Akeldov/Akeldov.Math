# ParameterizedLine

`ParameterizedLine` is an infinite directed line with an `Origin`, `Direction`, and signed curve coordinate.

Coordinates are measured from `Origin` along `Direction` in world coordinate units.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var geometricLine = new Line(
    new PointXY(0f, 2f),
    new PointXY(6f, 2f));

var path = new ParameterizedLine(
    geometricLine,
    referencePoint: new PointXY(3f, 10f));

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(5f, 5f));

PointXY projectedPoint = projection.ProjectedPoint;  // (5, 2)
float curveCoordinate = projection.CurveCoordinate;  // 2
PointXY beforeOrigin = path.GetPoint(-1f);           // (2, 2)
```

You can choose the coordinate origin with an explicit reference point or with `LineReferencePointMode`.

```csharp
var centered = new ParameterizedLine(
    new PointXY(0f, 2f),
    new PointXY(6f, 2f),
    LineReferencePointMode.Midpoint);
```
