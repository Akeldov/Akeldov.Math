# Segment

`Segment` is a finite curve between two endpoints.
It is endpoint-order agnostic, but endpoint inclusion is still preserved.

Use `Segment` when endpoint order should not matter and you only need geometric operations.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var closed = new Segment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

var openAtStart = new Segment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f),
    includesEndpointA: false,
    includesEndpointB: true);

CurveProjection projection = closed.Project(new PointXY(4f, 3f));
Segment shorter = closed.Shorten(1f);
Segment longer = closed.Extend(2f);
```

Endpoint inclusion matters for ray intersections at exact endpoints.

Degenerate segments are allowed. For a zero-length `Segment`, projection returns the endpoint.

Use [`ParameterizedSegment`](parameterized-segment.md) when you need traversal direction or a coordinate from start to end.
