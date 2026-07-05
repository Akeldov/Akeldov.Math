# Ray

`Ray` starts at `Origin` and extends forever in `Direction`.
Its coordinate domain is `[0, +inf)`.

The `angle` constructor argument and `Angle` property are expressed in radians.

This code uses the ray coordinate to generate the documentation image below.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var ray = new Ray(
    origin: new PointXY(-2.45f, -2.05f),
    angle: MathF.PI / 5f);

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

        return (byte)MathF.Round(normalized * byte.MaxValue);
    });

SpatialRaster<byte> raster = ray.Rasterize(grid, rasterizer);
raster.SaveAsPng("ray-growing-thickness.png");
```

<p>
  <img class="curve-snapshot" alt="Ray growing-thickness raster" src="../../../../assets/spatial2d/curves/ray-growing-thickness.png">
</p>

Points that would project behind the origin clamp to the origin.

```csharp
var ray = new Ray(
    origin: new PointXY(0f, 0f),
    angle: MathF.PI / 4f);

ParameterizedCurveProjection projection =
    ray.ProjectWithParameter(new PointXY(-1f, 2f));

PointXY start = ray.GetPoint(0f);
PointXY fiveUnitsAlongRay = ray.GetPoint(5f);
```

Use `Direction` when you want the normalized vector form.
