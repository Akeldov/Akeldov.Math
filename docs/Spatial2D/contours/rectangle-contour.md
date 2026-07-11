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
