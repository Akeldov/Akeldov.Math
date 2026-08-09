# ParameterizedRectangleContour

`ParameterizedRectangleContour` is an axis-aligned rectangular boundary with a length coordinate around its perimeter.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new ParameterizedRectangleContour(
    new PointXY(0f, 0f),
    new PointXY(4f, 2f),
    RectangleContourParameterOrigin.BottomLeft,
    ContourDirection.Counterclockwise);

PointXY origin = contour.ParameterOrigin;
PointXY point = contour.GetPoint(3f);
ParameterizedCurveProjection projection =
    contour.ProjectWithParameter(new PointXY(5f, 1f));
```

By default, coordinate `0` is the right-edge midpoint and coordinates increase counterclockwise. A constructor can instead select a named `RectangleContourParameterOrigin` or a counterclockwise boundary coordinate measured from that default origin. The direction may be clockwise or counterclockwise.

The contour exposes its normalized bounds, dimensions, center, corners, parameter origin, direction, and perimeter length. It supports all regular contour queries plus `GetPoint` and `ProjectWithParameter`.

`Rectangle` and `ToRegion` return the filled rectangle. Explicit conversion to `RectangleContour` removes the parameterization; conversion to `Rectangle` returns the region.

## Rasterization example

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

IContour contour = new ParameterizedRectangleContour(new PointXY(-2f, -1.4f), new PointXY(2f, 1.4f));
var grid = new RasterGeometry(new PointXY(-3f, -3f), new VectorXY(6f, 6f), new VectorXYInt(96, 96));
contour.Rasterize(0.08f, 0.08f, new Gray8BitColor(byte.MaxValue), grid).SaveAsPng("parameterized-rectangle-contour.png");
```

![Parameterized rectangle contour raster](../../assets/spatial2d/contours/parameterized-rectangle-contour-growing-thickness.png)
