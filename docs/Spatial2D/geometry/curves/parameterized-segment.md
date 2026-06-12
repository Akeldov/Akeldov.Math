# ParameterizedSegment

`ParameterizedSegment` is a directed finite path.
Its coordinate starts at `0` at `StartPoint` and ends at `Length` at `EndPoint`.

Use it when traversal direction or distance along the segment matters.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(4f, 3f));

float curveCoordinate = projection.CurveCoordinate; // 4
PointXY halfway = path.GetPoint(path.Length * 0.5f);
Segment geometricSegment = (Segment)path;
```

Reversing the endpoints reverses the coordinate domain.

For a zero-length `ParameterizedSegment`, coordinate `0` returns `StartPoint`.
