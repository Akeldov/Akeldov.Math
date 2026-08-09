# Circle

`Circle` is an immutable circular boundary implementing `IContour`. It stores a center and a non-negative radius; use [`Disk`](../regions/disk.md) when a filled circular region is the primary abstraction.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var circle = new Circle(new PointXY(2f, 3f), radius: 5f);

float length = circle.Length; // 2πr
bool enclosed = circle.Encloses(new PointXY(4f, 3f));
float distance = circle.Distance(new PointXY(8f, 3f)); // 1
CurveProjection projection = circle.Project(new PointXY(8f, 3f));
```

`Center`, `Radius`, and `Length` describe the boundary. `Encloses` classifies a point against the enclosed disk, while `Distance` measures the unsigned distance to the circumference and `SignedDistance` is negative inside. `GetRayIntersections` returns a new caller-owned mutable list of intersections.

When projecting the center of a nonzero circle, the positive X-axis point is used as a stable result. A zero-radius circle projects to its center.

`Circle` supports value equality and invariant-culture `ToString`. Use [`ParameterizedCircle`](parameterized-circle.md) when points must also be addressed by a length coordinate around the circumference.

## Rasterization example

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

IContour contour = new Circle(new PointXY(0.1f, -0.15f), 1.75f);
var grid = new RasterGeometry(new PointXY(-3f, -3f), new VectorXY(6f, 6f), new VectorXYInt(96, 96));
contour.Rasterize(0.08f, 0.08f, new Gray8BitColor(byte.MaxValue), grid).SaveAsPng("circle.png");
```

![Circle contour raster](../../assets/spatial2d/curves/circle-distance.png)
