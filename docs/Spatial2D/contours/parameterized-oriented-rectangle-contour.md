# ParameterizedOrientedRectangleContour

`ParameterizedOrientedRectangleContour` combines an oriented rectangular boundary with a length coordinate around its perimeter.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new ParameterizedOrientedRectangleContour(
    center: new PointXY(0f, 0f),
    size: new VectorXY(6f, 2f),
    rotation: MathF.PI / 6f,
    parameterOrigin: RectangleContourParameterOrigin.TopLeft,
    contourDirection: ContourDirection.Clockwise);

PointXY origin = contour.ParameterOrigin;
PointXY point = contour.GetPoint(2f);
```

Rotation is expressed in radians. By default, coordinate `0` is the right-edge midpoint and traversal is counterclockwise. Other constructors accept a named rectangular boundary point or a counterclockwise boundary coordinate measured from the default origin.

The type exposes the center, size, rotation, local axes, corners, parameter origin, direction, and perimeter. In addition to regular contour queries, it provides `GetPoint` and `ProjectWithParameter`.

Use `ToRegion` or `Rectangle` for the filled oriented rectangle. Explicit conversions to `OrientedRectangleContour` and `OrientedRectangle` remove parameterization.

## Rasterization example

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

IContour contour = new ParameterizedOrientedRectangleContour(new PointXY(0f, 0f), new VectorXY(4f, 2.2f), MathF.PI / 6f);
var grid = new SpatialRasterGrid(new PointXY(-3f, -3f), new VectorXY(6f, 6f), new VectorXYInt(96, 96));
contour.Rasterize(0.08f, 0.08f, new Gray8BitColor(byte.MaxValue), grid).SaveAsPng("parameterized-oriented-rectangle-contour.png");
```

![Parameterized oriented rectangle contour raster](../../assets/spatial2d/contours/parameterized-oriented-rectangle-contour-growing-thickness.png)
