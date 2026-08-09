# Circular Contours

Circular contours represent a complete circumference defined by a center and radius. Distance and projection operate on the boundary, while `Encloses` tests membership in the enclosed disk.

## Choosing a Type

| Type | Curve coordinate | Use when |
| --- | --- | --- |
| [`Circle`](../circle.md) | None | Only circular geometry, distance, projection, intersections, or enclosure is required. |
| [`ParameterizedCircle`](../parameterized-circle.md) | `[0, Length]` | A coordinate origin and clockwise or counterclockwise traversal are required. |

`ParameterizedCircle` adds `StartAngle` and `ContourDirection` without changing the underlying circumference. Angles are expressed in radians by default; `StartAngleDeg` exposes the normalized start angle in degrees.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var contour = new ParameterizedCircle(
    center: new PointXY(0f, 0f),
    radius: 2f,
    startAngle: 0f,
    contourDirection: ContourDirection.Counterclockwise);

PointXY start = contour.GetPoint(0f);                    // (2, 0)
PointXY quarter = contour.GetPoint(contour.Length / 4f); // (0, 2)
float circumference = contour.Length;                   // 4 * PI
```

Curve coordinates are measured in world coordinate units along the circumference and wrap at `Length`. For a bounded circular span with distinct endpoints, use [`Arc`](../../curves/circular/arc.md) or [`ParameterizedArc`](../../curves/circular/parameterized-arc.md).
