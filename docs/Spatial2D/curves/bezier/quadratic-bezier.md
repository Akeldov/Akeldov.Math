# QuadraticBezier

`QuadraticBezier` is a finite directed Bezier curve segment with one control point.
It starts at `StartPoint`, ends at `EndPoint`, and bends toward `ControlPoint`.

Use it for TrueType-style outlines, simple smooth joins, or any finite curved path that only needs one control handle.

`GetPointAt` evaluates the normalized Bezier parameter `t` in the `[0, 1]` range.
`GetPoint` and `ProjectWithParameter` use the approximate length coordinate from `0` through `Length`.
Length, projection, distance, and ray-intersection operations use a fixed internal polyline approximation.

This code uses the curve coordinate to generate the documentation image below.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var curve = new QuadraticBezier(
    startPoint: new PointXY(-2.25f, -1.85f),
    controlPoint: new PointXY(-0.25f, 2.45f),
    endPoint: new PointXY(2.25f, -1.35f));

var grid = new SpatialRasterGrid(
    origin: new PointXY(-3f, -3f),
    size: new VectorXY(6f, 6f),
    resolution: new VectorXYInt(192, 192));

var rasterizer = new ParameterizedCurveDistanceGray8BitRasterizer(
    (distance, curveCoordinate) =>
    {
        const float baseThickness = 0.05f;
        const float thicknessPerWorldUnit = 0.065f;
        const float maxThicknessGrowth = 0.42f;
        const float edgeFalloff = 0.08f;

        float nonNegativeCoordinate = MathF.Max(0f, curveCoordinate);
        float thickness = baseThickness + MathF.Min(nonNegativeCoordinate * thicknessPerWorldUnit, maxThicknessGrowth);
        float normalized = 1f - Math.Clamp((distance - thickness) / edgeFalloff, 0f, 1f);

        return new Gray8BitColor((byte)MathF.Round(normalized * byte.MaxValue));
    });

SpatialRaster<Gray8BitColor> raster = curve.Rasterize(grid, rasterizer);
raster.SaveAsPng("quadratic-bezier-growing-thickness.png");
```

![Quadratic Bezier growing-thickness raster](../../../assets/spatial2d/curves/quadratic-bezier-growing-thickness.png)

```csharp
var curve = new QuadraticBezier(
    startPoint: new PointXY(0f, 0f),
    controlPoint: new PointXY(1f, 2f),
    endPoint: new PointXY(2f, 0f));

PointXY start = curve.GetPointAt(0f);     // (0, 0)
PointXY middle = curve.GetPointAt(0.5f);  // (1, 1)
PointXY end = curve.GetPointAt(1f);       // (2, 0)

PointXY halfwayByLength = curve.GetPoint(curve.Length * 0.5f);
ParameterizedCurveProjection projection =
    curve.ProjectWithParameter(new PointXY(1f, 2f));

List<ParameterizedSegment> approximation = curve.Flatten(16);
```

`Flatten` returns a new mutable list of directed segments owned by the caller.
