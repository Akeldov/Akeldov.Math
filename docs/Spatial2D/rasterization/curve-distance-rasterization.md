# Curve Distance Rasterization

Curve distance rasterizers convert distance from a curve into a grayscale value.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var curve = new ParameterizedSegment(
    new PointXY(0f, 0f),
    new PointXY(4f, 0f));

var grid = new SpatialRasterGrid(
    origin: new PointXY(-0.5f, -1f),
    size: new VectorXY(5f, 2f),
    resolution: new VectorXYInt(160, 64));

var rasterizer = new ParameterizedCurveDistanceGray8BitRasterizer(
    (distance, curveCoordinate) =>
    {
        float normalized = 1f - Math.Clamp(distance / 0.25f, 0f, 1f);
        return new Gray8BitColor((byte)MathF.Round(normalized * byte.MaxValue));
    });

SpatialRaster<Gray8BitColor> raster = curve.Rasterize(grid, rasterizer);
```
