# Circular Curves

Circular curves are defined by a center and radius. Arcs additionally restrict the circumference to an angular span. Angles are expressed in radians by default; degree-based properties use the `Deg` suffix.

## Choosing a Type

| Type | Geometry | Curve coordinate | Use when |
| --- | --- | --- | --- |
| [`Circle`](circle.md) | Full circumference | None | A complete circular boundary is required without a traversal origin. |
| [`Arc`](arc.md) | Bounded angular span | None | Only the geometry between two angles matters. |
| [`ParameterizedArc`](parameterized-arc.md) | Directed angular span | `[0, Length]` | Clockwise or counterclockwise traversal and distance from the start point matter. |

`Circle` belongs to the `Akeldov.Math.Spatial2D.Contours` namespace because a full circumference is a closed contour. `Arc` and `ParameterizedArc` belong to `Akeldov.Math.Spatial2D.Curves`. For a directed full circumference, see [`ParameterizedCircle`](../../contours/parameterized-circle.md).

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var arc = new ParameterizedArc(
    center: new PointXY(0f, 0f),
    radius: 2f,
    startAngle: 0f,
    endAngle: MathF.PI,
    angularDirection: AngularDirection.Counterclockwise);

PointXY start = arc.GetPoint(0f);                    // (2, 0)
PointXY halfway = arc.GetPoint(arc.Length * 0.5f);   // (0, 2)
PointXY end = arc.GetPoint(arc.Length);              // (-2, 0)
```

Projection onto a bounded arc uses the source circle while the point direction lies inside the angular region. Outside that region, projection clamps to the nearest arc endpoint.
