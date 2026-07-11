# RectangleContour

`RectangleContour` is the closed boundary of an axis-aligned rectangle. Two opposite corners may be supplied in any order; the contour normalizes them into `Min` and `Max`.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var contour = new RectangleContour(
    new PointXY(4f, 3f),
    new PointXY(0f, 0f));

PointXY min = contour.Min;       // (0, 0)
PointXY max = contour.Max;       // (4, 3)
VectorXY size = contour.Size;    // (4, 3)
float perimeter = contour.Length; // 14
```

The type exposes `Width`, `Height`, `Size`, `Center`, and all four named corners. `Encloses` tests exact membership in the bounded rectangle. `Distance`, `SignedDistance`, `Project`, and `GetRayIntersections` operate on its boundary.

`Rectangle` and `ToRegion` return the corresponding filled region. Explicit conversions are available to `Rectangle` and [`ParameterizedRectangleContour`](parameterized-rectangle-contour.md). Value equality compares the normalized bounds.

## Rasterization example

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

IContour contour = new RectangleContour(new PointXY(-2f, -1.4f), new PointXY(2f, 1.4f));
var grid = new SpatialRasterGrid(new PointXY(-3f, -3f), new VectorXY(6f, 6f), new VectorXYInt(96, 96));
contour.Rasterize(0.08f, 0.08f, new Gray8BitColor(byte.MaxValue), grid).SaveAsPng("rectangle-contour.png");
```

![Rectangle contour raster](../../assets/spatial2d/contours/rectangle-contour-distance.png)
