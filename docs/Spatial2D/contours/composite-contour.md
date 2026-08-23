# CompositeContour

`CompositeContour` joins contour paths into one closed boundary. It implements `ICompositeContour` and retains its own array of path references behind a read-only structural view. Every `IContourPath` is finite and directed, supports fill-rule crossings, and directly provides the ray-intersection query needed by the composite contour.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new CompositeContour(new IContourPath[]
{
    new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(4f, 0f)),
    new ParameterizedSegment(new PointXY(4f, 0f), new PointXY(2f, 3f)),
    new ParameterizedSegment(new PointXY(2f, 3f), new PointXY(0f, 0f))
});
```

Adjacent paths must connect and the final endpoint must meet the first start point. At least one contour path is required, all paths must have finite non-negative lengths, and the accumulated contour length must remain finite.

For polygonal contours, pass at least three vertices instead. Consecutive vertices are connected with `ParameterizedSegment` edges and the last vertex is connected back to the first:

```csharp
var polygon = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(4f, 0f),
    new PointXY(2f, 3f));
```

`Curves` exposes the read-only structural view and `Length` returns their total length. The contour supports enclosure, crossings, projection, and signed or unsigned distance. Ray intersections for the concrete `CompositeContour` type are available through its extension method. Use [`ParameterizedCompositeContour`](parameterized-composite-contour.md) when a continuous length coordinate is required across the constituent paths.

## Rasterization example

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

IContour contour = new CompositeContour(new PointXY(-2f, -1.5f), new PointXY(2f, -1.5f), new PointXY(0f, 2f));
var grid = new RasterGeometry(new PointXY(-3f, -3f), new VectorXY(6f, 6f), new VectorXYInt(96, 96));
contour.Rasterize(0.08f, 0.08f, new Gray8BitColor(byte.MaxValue), grid).SaveAsPng("composite-contour.png");
```

![Composite contour raster](../../assets/spatial2d/contours/composite-contour-distance.png)
