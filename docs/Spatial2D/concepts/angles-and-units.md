# Angles and Units

Angles are expressed in radians by default throughout `Akeldov.Math.Spatial2D`.

Members that use degrees have an explicit `Deg` suffix, such as `StartAngleDeg` and `EndAngleDeg`.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var ray = new Ray(
    origin: new PointXY(0f, 0f),
    angle: MathF.PI / 4f);

float angleRad = ray.Angle;
```

Curve coordinates, distances, radii, grid sizes, and raster cell locations are measured in world coordinate units unless a member says otherwise.
