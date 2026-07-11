# CubicBezier

`CubicBezier` is a finite directed Bezier curve segment with two control points.
It starts at `StartPoint`, ends at `EndPoint`, and uses `ControlPointA` and `ControlPointB` as handles for the curve shape.

Use it for vector drawing style curves, font outlines that use cubic segments, or smooth paths that need separate outgoing and incoming handles.

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

var curve = new CubicBezier(
    startPoint: new PointXY(-2.5f, -2.1f),
    controlPointA: new PointXY(-2.15f, 2.3f),
    controlPointB: new PointXY(2.35f, 2.1f),
    endPoint: new PointXY(2.5f, -1.8f));

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
raster.SaveAsPng("cubic-bezier-growing-thickness.png");
```

![Cubic Bezier growing-thickness raster](../../../assets/spatial2d/curves/cubic-bezier-growing-thickness.png)

Control points shape the curve but the curve does not usually pass through them.

```csharp
var curve = new CubicBezier(
    startPoint: new PointXY(0f, 0f),
    controlPointA: new PointXY(0f, 3f),
    controlPointB: new PointXY(3f, 3f),
    endPoint: new PointXY(3f, 0f));

PointXY start = curve.GetPointAt(0f);     // (0, 0)
PointXY middle = curve.GetPointAt(0.5f);  // (1.5, 2.25)
PointXY end = curve.GetPointAt(1f);       // (3, 0)

PointXY halfwayByLength = curve.GetPoint(curve.Length * 0.5f);
ParameterizedCurveProjection projection =
    curve.ProjectWithParameter(new PointXY(1.5f, 3f));

List<ParameterizedSegment> approximation = curve.Flatten(24);
```

`Flatten` returns a new mutable list of directed segments owned by the caller.
