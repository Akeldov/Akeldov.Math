# ParameterizedArc

`ParameterizedArc` adds `AngularDirection` and a length-based curve coordinate to circular arc geometry.

The coordinate starts at `0` at `StartPoint` and ends at `Length` at `EndPoint`.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var path = new ParameterizedArc(
    center: new PointXY(0f, 0f),
    radius: 2f,
    startAngle: 0f,
    endAngle: MathF.PI,
    angularDirection: AngularDirection.Counterclockwise);

PointXY halfway = path.GetPoint(path.Length * 0.5f); // (0, 2)

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(0f, 3f));

PointXY projectedPoint = projection.ProjectedPoint; // (0, 2)
float curveCoordinate = projection.CurveCoordinate; // PI
float distance = projection.Distance;               // 1
Arc geometricArc = (Arc)path;
```

`AngularDirection.Counterclockwise` increases the angle from `StartAngle` toward `EndAngle`.
`AngularDirection.Clockwise` traverses from the same start point in the opposite direction.

`GetPoint` accepts coordinates from `0` through `Length`. Coordinates outside that range, plus NaN or infinite coordinates, throw `ArgumentOutOfRangeException`.

`ParameterizedArc` exposes `StartAngleDeg`, `EndAngleDeg`, and `ToDegreesString()` when degree output is more convenient.
