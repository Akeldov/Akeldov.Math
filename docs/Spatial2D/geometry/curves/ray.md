# Ray

`Ray` starts at `Origin` and extends forever in `Direction`.
Its coordinate domain is `[0, +inf)`.

The `angle` constructor argument and `Angle` property are expressed in radians.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var ray = new Ray(
    origin: new PointXY(0f, 0f),
    angle: MathF.PI / 4f);

ParameterizedCurveProjection projection =
    ray.ProjectWithParameter(new PointXY(-1f, 2f));

PointXY start = ray.GetPoint(0f);
PointXY fiveUnitsAlongRay = ray.GetPoint(5f);
```

Points that would project behind the origin clamp to the origin.

Use `Direction` when you want the normalized vector form.
