# ParameterizedArc

`ParameterizedArc` adds `AngularDirection` and a length-based curve coordinate to circular arc geometry.

The coordinate starts at `0` at `StartPoint` and ends at `Length` at `EndPoint`.

This code uses the arc coordinate to generate the documentation image below.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var path = new ParameterizedArc(
    center: new PointXY(0f, 0f),
    radius: 2f,
    startAngle: -MathF.PI / 4f,
    endAngle: 5f * MathF.PI / 4f,
    angularDirection: AngularDirection.Counterclockwise);

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

SpatialRaster<Gray8BitColor> raster = path.Rasterize(grid, rasterizer);
raster.SaveAsPng("parameterized-arc-growing-thickness.png");
```

![Parameterized arc growing-thickness raster](../../../assets/spatial2d/curves/parameterized-arc-growing-thickness.png)

`AngularDirection.Counterclockwise` increases the angle from `StartAngle` toward `EndAngle`.
`AngularDirection.Clockwise` traverses from the same start point in the opposite direction.

```csharp
var path = new ParameterizedArc(
    center: new PointXY(0f, 0f),
    radius: 2f,
    startAngle: 0f,
    endAngle: MathF.PI,
    angularDirection: AngularDirection.Counterclockwise);

PointXY halfway = path.GetPoint(path.Length * 0.5f); // (0, 2)

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(0f, 3f));

PointXY projectedPoint = projection.ProjectedPoint; // (0, 2)
float curveCoordinate = projection.CurveCoordinate; // PI
float distance = projection.Distance;               // 1
Arc geometricArc = (Arc)path;
```

`GetPoint` accepts coordinates from `0` through `Length`. Coordinates outside that range, plus NaN or infinite coordinates, throw `ArgumentOutOfRangeException`.

`ParameterizedArc` exposes `StartAngleDeg`, `EndAngleDeg`, and `ToDegreesString()` when degree output is more convenient.
