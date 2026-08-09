# Linear Curves

Linear curves represent straight one-dimensional geometry. Choose a type by answering two questions: whether the geometry is bounded, and whether traversal direction or a curve coordinate matters.

## Choosing a Type

| Type | Extent | Curve coordinate | Use when |
| --- | --- | --- | --- |
| [`Line`](line.md) | Infinite in both directions | None | Only geometric distance, projection, intersections, or side tests are needed. |
| [`ParameterizedLine`](parameterized-line.md) | Infinite in both directions | `(-∞, +∞)` | An origin, direction, and signed distance along the line are required. |
| [`Ray`](ray.md) | Starts at an origin and extends infinitely | `[0, +∞)` | Geometry behind the origin must be excluded. |
| [`Segment`](segment.md) | Between two endpoints | None | Endpoint order should not affect geometric identity. |
| [`ParameterizedSegment`](parameterized-segment.md) | Between a start and end point | `[0, Length]` | Traversal direction or distance from the start point matters. |
| [`ParameterizedSegmentChain`](parameterized-segment-chain.md) | Open chain of consecutive segments | `[0, Length]` | An open polyline should behave as one finite path. |

Parameterized coordinates are measured in world coordinate units. A coordinate of `0` identifies the origin or start point, and increasing coordinates follow the stored direction.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var line = new Line(
    new PointXY(0f, 0f),
    new PointXY(4f, 2f));

var ray = new Ray(
    origin: new PointXY(0f, 0f),
    angle: 0f);

var segment = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(4f, 0f));

PointXY halfway = segment.GetPoint(segment.Length * 0.5f); // (2, 0)
```

All linear curve types support point distance and projection. Use the parameterized variants when the projected result must also include a meaningful position along the curve.
