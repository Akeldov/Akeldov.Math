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

## Rasterization example

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

IContour contour = new OrientedRectangleContour(new PointXY(0f, 0f), new VectorXY(4f, 2.2f), MathF.PI / 6f);
var grid = new SpatialRasterGrid(new PointXY(-3f, -3f), new VectorXY(6f, 6f), new VectorXYInt(96, 96));
contour.Rasterize(0.08f, 0.08f, new Gray8BitColor(byte.MaxValue), grid).SaveAsPng("oriented-rectangle-contour.png");
```

![Oriented rectangle contour raster](../../assets/spatial2d/contours/oriented-rectangle-contour-distance.png)
