# OrientedRectangleContour

`OrientedRectangleContour` is a rectangular boundary centered at a point and rotated relative to the world axes.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var contour = new OrientedRectangleContour(
    center: new PointXY(1f, 2f),
    size: new VectorXY(6f, 3f),
    rotation: MathF.PI / 4f);

VectorXY localX = contour.AxisX;
VectorXY localY = contour.AxisY;
bool enclosed = contour.Encloses(new PointXY(1f, 2f));
```

`Rotation` is the counterclockwise angle of the local X axis in radians. Both size components must be positive. The type exposes `Center`, `Size`, `Width`, `Height`, local axes, four world-space corners, and perimeter `Length`.

Boundary operations include ray intersections, projection, unsigned distance, and signed distance. `Rectangle` and `ToRegion` return an `OrientedRectangle`. Explicit conversion to [`ParameterizedOrientedRectangleContour`](parameterized-oriented-rectangle-contour.md) uses its default parameter origin and direction.
