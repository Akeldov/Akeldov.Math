# Line

`Line` represents an infinite geometric line.
It has no start point and no curve coordinate.

Use `Line` when you only need geometric distance, projection, ray intersections, or side tests.

This code uses the same line, raster grid, and distance mapping as the documentation image below.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var line = new Line(
    new PointXY(-2.5f, -1.5f),
    new PointXY(2.5f, 1.75f));

var grid = new SpatialRasterGrid(
    origin: new PointXY(-3f, -3f),
    size: new VectorXY(6f, 6f),
    resolution: new VectorXYInt(192, 192));

var rasterizer = new PointDistanceProviderGray8BitRasterizer(distance =>
{
    const float falloffDistance = 0.25f;
    float normalized = 1f - Math.Clamp(distance / falloffDistance, 0f, 1f);
    return (byte)MathF.Round(normalized * byte.MaxValue);
});

SpatialRaster<byte> raster = line.Rasterize(grid, rasterizer);
raster.SaveAsPng("line-distance.png");
```

![Line distance raster](../../../assets/spatial2d/curves/line-distance.png)

You can also construct a line from implicit equation coefficients.

```csharp
var horizontal = new Line(a: 0f, b: 1f, c: -2f);
CurveProjection projection = horizontal.Project(new PointXY(4f, 5f));
```

The implicit equation coefficients are normalized, so equivalent equations compare as the same line.

You can construct a line from a point and direction angle in radians.

```csharp
var diagonal = new Line(
    new PointXY(0f, 0f),
    angle: MathF.PI / 4f);
```

Use [`ParameterizedLine`](parameterized-line.md) when you need an origin, direction, and signed coordinate along the same infinite geometry.
