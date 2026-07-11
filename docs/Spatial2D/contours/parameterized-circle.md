# ParameterizedCircle

`ParameterizedCircle` adds a length-based curve coordinate to a circular contour. Coordinate `0` lies at `StartAngle`, and coordinates increase in the selected `ContourDirection`.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var circle = new ParameterizedCircle(
    center: new PointXY(0f, 0f),
    radius: 3f,
    startAngle: MathF.PI / 2f,
    contourDirection: ContourDirection.Counterclockwise);

PointXY start = circle.GetPoint(0f); // approximately (0, 3)
ParameterizedCurveProjection projection =
    circle.ProjectWithParameter(new PointXY(-4f, 0f));
```

Angles are in radians; `StartAngleDeg` exposes the start angle in degrees. `Length` is the circumference, and `GetPoint` accepts coordinates from `0` through `Length`. The type also exposes `Circle`, `Center`, `Radius`, `StartAngle`, and `ContourDirection`.

All non-parameterized contour operations are available, including enclosure, ray intersections, projection, and signed or unsigned distance. Explicit conversion to `Circle` removes only the parameterization.
