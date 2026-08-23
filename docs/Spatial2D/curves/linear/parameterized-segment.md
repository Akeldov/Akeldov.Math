# ParameterizedSegment

`ParameterizedSegment` is a directed finite path.
Its coordinate starts at `0` at `StartPoint` and ends at `Length` at `EndPoint`.

Use it when traversal direction or distance along the segment matters.

This code uses the segment coordinate to generate the documentation image below.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var path = new ParameterizedSegment(
    startPoint: new PointXY(-2.35f, -2.1f),
    endPoint: new PointXY(2.35f, 1.75f));

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
raster.SaveAsPng("parameterized-segment-growing-thickness.png");
```

![Parameterized segment growing-thickness raster](../../../assets/spatial2d/curves/parameterized-segment-growing-thickness.png)

Reversing the endpoints reverses the coordinate domain.

```csharp
var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(4f, 3f));

float curveCoordinate = projection.CurveCoordinate; // 4
PointXY halfway = path.GetPoint(path.Length * 0.5f);
PointXY normalizedHalfway = path.GetPointAtNormalizedCoordinate(0.5f);
ParameterizedSegment shorter = path.Shorten(1f);
ParameterizedSegment shorterAtStart = path.ShortenStart(1f);
ParameterizedSegment longer = path.Extend(2f);
ParameterizedSegment longerAtEnd = path.ExtendEnd(2f);
Segment geometricSegment = (Segment)path;
```

For a zero-length `ParameterizedSegment`, coordinate `0` returns `StartPoint`.
