# BezierCurve

`BezierCurve` is a finite directed Bezier curve segment of arbitrary degree.
It is useful when control point count is data-driven or when a curve should be built from an existing control point collection.

Use `QuadraticBezier` or `CubicBezier` for the common fixed-degree cases.
Use `BezierCurve` when the degree is not known at compile time or when a higher-degree curve is intentional.

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

var curve = new BezierCurve(
    new PointXY(-2.35f, -1.9f),
    new PointXY(-1.4f, 2.25f),
    new PointXY(0.15f, -2.3f),
    new PointXY(1.5f, 2.15f),
    new PointXY(2.35f, -1.65f));

var grid = new RasterGeometry(
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
raster.SaveAsPng("bezier-curve-growing-thickness.png");
```

![Arbitrary-degree Bezier growing-thickness raster](../../../assets/spatial2d/curves/bezier-curve-growing-thickness.png)

The constructor copies its input points.
`ControlPoints` is a read-only structural view of that copied state.

```csharp
var controlPoints = new[]
{
    new PointXY(0f, 0f),
    new PointXY(0f, 3f),
    new PointXY(3f, 3f),
    new PointXY(3f, 0f)
};

var curve = new BezierCurve(controlPoints);
controlPoints[1] = new PointXY(10f, 10f);

int degree = curve.Degree;                       // 3
PointXY middle = curve.GetPointAt(0.5f);         // (1.5, 2.25)
PointXY halfwayByLength = curve.GetPoint(curve.Length * 0.5f);

ParameterizedCurveProjection projection =
    curve.ProjectWithParameter(new PointXY(1.5f, 3f));

List<ParameterizedSegment> approximation = curve.Flatten(24);
```

`BezierCurve` requires at least two finite control points.
`Flatten` returns a new mutable list of directed segments owned by the caller.
