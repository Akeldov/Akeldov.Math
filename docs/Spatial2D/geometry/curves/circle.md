# Circle

`Circle` represents a full circumference with a `Center`, `Radius`, and `Length`.

Distance and projection are measured to the circumference, not to a filled disk.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(
    center: new PointXY(1f, 1f),
    radius: 2f);

CurveProjection projection = circle.Project(new PointXY(4f, 1f));

PointXY closestPoint = projection.ProjectedPoint; // (3, 1)
float distance = projection.Distance;             // 1
float circumference = circle.Length;              // 4 * PI
```

If the projected point is exactly at the circle center, projection uses the point on the positive X axis.
If the radius is zero, projection returns the center.
