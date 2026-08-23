# Bezier Curves

Bezier curves are finite directed paths shaped by endpoints and control points. The library provides fixed-degree value types for common cases and an arbitrary-degree type for data-driven control point collections.

## Choosing a Type

| Type | Degree | Control points | Use when |
| --- | ---: | ---: | --- |
| [`QuadraticBezier`](quadratic-bezier.md) | 2 | 1 | A simple bend or TrueType-style quadratic outline is needed. |
| [`CubicBezier`](cubic-bezier.md) | 3 | 2 | Separate outgoing and incoming handles are needed for vector drawing paths. |

All three types implement `IFinitePath`. `StartPoint` and `EndPoint` define traversal direction, and the control points shape the curve without usually lying on it.

## Parameters and Coordinates

Bezier APIs expose two different coordinates:

- `GetPointAt(t)` uses the normalized Bezier parameter `t` in `[0, 1]`.
- `GetPoint(curveCoordinate)` uses the approximate length coordinate in `[0, Length]`.

Length, projection, distance, and ray intersections use the library's internal polyline approximation. `Flatten` exposes an explicit caller-owned mutable segment approximation when a chosen segment count is required.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var curve = new CubicBezier(
    startPoint: new PointXY(0f, 0f),
    controlPointA: new PointXY(0f, 3f),
    controlPointB: new PointXY(3f, 3f),
    endPoint: new PointXY(3f, 0f));

PointXY middleByParameter = curve.GetPointAt(0.5f);          // (1.5, 2.25)
PointXY middleByLength = curve.GetPoint(curve.Length * 0.5f);
```
