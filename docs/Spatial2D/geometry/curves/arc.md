# Arc

`Arc` represents a bounded part of a circle.

The `startAngle` and `endAngle` constructor arguments, plus the `StartAngle` and `EndAngle` properties, are in radians.
Stored angles are normalized.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var arc = new Arc(
    center: new PointXY(0f, 0f),
    radius: 5f,
    startAngle: 0f,
    endAngle: MathF.PI / 2f);

bool isInsideArcAngle = arc.IsWithinAngularRegion(new PointXY(1f, 1f));
PointXY start = arc.StartPoint; // (5, 0)
PointXY end = arc.EndPoint;     // (0, 5)

CurveProjection projection = arc.Project(new PointXY(-3f, 4f));
```

When a point's direction from the center is inside the arc's angular region, projection lands on the source circle.
When the direction is outside the angular region, projection clamps to the nearest endpoint.

Equal input angles create a zero-length arc at the start point.
An end angle one full turn after the start angle creates a full circle, even though normalized start and end angles are equal.
