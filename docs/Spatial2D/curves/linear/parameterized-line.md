# ParameterizedLine

`ParameterizedLine` is an infinite directed line with an `Origin`, `Direction`, and signed curve coordinate.

Coordinates are measured from `Origin` along `Direction` in world coordinate units.

This code uses the signed curve coordinate to generate the documentation image below.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var path = new ParameterizedLine(
    new PointXY(-0.4f, -2.65f),
    new VectorXY(0.45f, 1f));

var grid = new RasterGeometry(
    origin: new PointXY(-3f, -3f),
    size: new VectorXY(6f, 6f),
    resolution: new VectorXYInt(192, 192));

var rasterizer = new ParameterizedCurveDistanceRasterizer<Gray8BitColor>(
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
raster.SaveAsPng("parameterized-line-growing-thickness.png");
```

![Parameterized line growing-thickness raster](../../../assets/spatial2d/curves/parameterized-line-growing-thickness.png)

You can project points onto the line and get their signed coordinate.

```csharp
var path = new ParameterizedLine(
    new PointXY(0f, 2f),
    new VectorXY(1f, 0f));

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(5f, 5f));

PointXY projectedPoint = projection.ProjectedPoint;  // (5, 2)
float curveCoordinate = projection.CurveCoordinate;  // 5
PointXY beforeOrigin = path.GetPoint(-1f);           // (-1, 2)
```

The direction can also be provided as an angle in radians.

```csharp
var angledPath = new ParameterizedLine(
    origin: new PointXY(0f, 0f),
    angle: MathF.PI / 6f);
```

You can choose the coordinate origin with an explicit reference point or with `LineReferencePointMode`.

```csharp
var centered = new ParameterizedLine(
    new PointXY(0f, 2f),
    new PointXY(6f, 2f),
    LineReferencePointMode.Midpoint);
```
