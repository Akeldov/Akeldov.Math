# ParameterizedSegmentChain

`ParameterizedSegmentChain` is an open finite path made from consecutive directed line segments. It treats an entire polyline as one `IFinitePath` with a single length coordinate.

Use it when a path contains multiple straight segments but projection, distance, intersections, and point lookup should operate on the complete chain.

## Length Coordinate

The coordinate starts at `0` at `StartPoint`, increases through each segment in traversal order, and ends at `Length` at `EndPoint`. Segment lengths are accumulated, so crossing a segment boundary does not reset the coordinate.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var chain = new ParameterizedSegmentChain(
    new PointXY(0f, 0f),
    new PointXY(3f, 0f),
    new PointXY(3f, 4f));

float length = chain.Length;                  // 7
PointXY onFirstSegment = chain.GetPoint(2f);  // (2, 0)
PointXY onSecondSegment = chain.GetPoint(5f); // (3, 2)
PointXY end = chain.GetPoint(chain.Length);   // (3, 4)
```

`GetPoint` accepts finite coordinates in `[0, Length]`. Values outside that range throw `ArgumentOutOfRangeException`.

## Projection

Projection checks every segment and returns the closest point together with its coordinate in the complete chain.

```csharp
ParameterizedCurveProjection projection =
    chain.ProjectWithParameter(new PointXY(5f, 2f));

PointXY projectedPoint = projection.ProjectedPoint; // (3, 2)
float curveCoordinate = projection.CurveCoordinate; // 5
float distance = projection.Distance;               // 2
```

`Project` returns the same projected point and distance without the chain coordinate. `Distance` returns only the shortest distance.

## Collection Ownership

The constructor copies its input points. `Points` and the generated `Segments` are exposed as read-only structural views, so callers cannot change the path after construction.

```csharp
var sourcePoints = new[]
{
    new PointXY(0f, 0f),
    new PointXY(2f, 0f),
    new PointXY(2f, 3f)
};

var copiedChain = new ParameterizedSegmentChain(sourcePoints);
sourcePoints[1] = new PointXY(10f, 10f);

PointXY retainedPoint = copiedChain.Points[1]; // (2, 0)
```

At least two finite points are required. Adjacent points must be distinct, and every generated segment must have a finite positive length.

`GetPointIntersections` returns a new mutable caller-owned list. Ray intersections deduplicate shared vertices using the library's standard geometry tolerance and are ordered in the forward direction of the ray. Line intersections use exact comparisons for deduplication and are ordered in the line's canonical direction.

Use [`ParameterizedSegment`](parameterized-segment.md) when the path contains only one directed segment.
