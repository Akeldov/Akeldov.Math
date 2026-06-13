# Line

`Line` represents an infinite geometric line.
It has no start point and no curve coordinate.

Use `Line` when you only need geometric distance, projection, ray intersections, or side tests.

![Line distance raster from the curve snapshot tests](../../../assets/spatial2d/curves/line-distance.png)

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var line = new Line(
    new PointXY(0f, 2f),
    new PointXY(6f, 2f));

var sameLine = new Line(a: 0f, b: 1f, c: -2f);

CurveProjection projection = line.Project(new PointXY(4f, 5f));

PointXY closestPoint = projection.ProjectedPoint; // (4, 2)
float distance = projection.Distance;             // 3
```

The implicit equation coefficients are normalized, so equivalent equations compare as the same line.

Use [`ParameterizedLine`](parameterized-line.md) when you need an origin, direction, and signed coordinate along the same infinite geometry.
