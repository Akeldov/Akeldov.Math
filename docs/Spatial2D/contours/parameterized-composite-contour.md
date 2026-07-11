# ParameterizedCompositeContour

`ParameterizedCompositeContour` gives a closed chain of finite paths one continuous length coordinate. Coordinate `0` begins at the first path's start point and advances through the paths in list order.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new ParameterizedCompositeContour(new IFinitePath[]
{
    new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(4f, 0f)),
    new ParameterizedSegment(new PointXY(4f, 0f), new PointXY(2f, 3f)),
    new ParameterizedSegment(new PointXY(2f, 3f), new PointXY(0f, 0f))
});

PointXY point = contour.GetPoint(2f);
ParameterizedCurveProjection projection =
    contour.ProjectWithParameter(new PointXY(2f, 1f));
```

The same closed-chain and length validation rules as [`CompositeContour`](composite-contour.md) apply. `Curves` is a read-only structural view, `Length` is the total path length, and `StartPoint` and `EndPoint` identify the closed path endpoints.

`GetPoint` maps a contour coordinate to the appropriate constituent path. `ProjectWithParameter` returns both the closest boundary point and its continuous contour coordinate. All non-parameterized contour operations remain available.

## Rasterization example

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

IContour contour = new ParameterizedCompositeContour(new IFinitePath[]
{
    new ParameterizedSegment(new PointXY(-2f, -1.5f), new PointXY(2f, -1.5f)),
    new ParameterizedSegment(new PointXY(2f, -1.5f), new PointXY(0f, 2f)),
    new ParameterizedSegment(new PointXY(0f, 2f), new PointXY(-2f, -1.5f))
});
var grid = new SpatialRasterGrid(new PointXY(-3f, -3f), new VectorXY(6f, 6f), new VectorXYInt(96, 96));
contour.Rasterize(0.08f, 0.08f, new Gray8BitColor(byte.MaxValue), grid).SaveAsPng("parameterized-composite-contour.png");
```

![Parameterized composite contour raster](../../assets/spatial2d/contours/parameterized-composite-contour-growing-thickness.png)
