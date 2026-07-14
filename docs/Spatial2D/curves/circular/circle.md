# Circle

`Circle` represents a full circumference contour with a `Center`, `Radius`, and `Length`.

Distance and projection are measured to the circumference, not to a filled disk.

This code uses the same circle, raster grid, and distance mapping as the documentation image below.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var circle = new Circle(
    center: new PointXY(0.1f, -0.15f),
    radius: 1.75f);

var grid = new RasterGeometry(
    origin: new PointXY(-3f, -3f),
    size: new VectorXY(6f, 6f),
    resolution: new VectorXYInt(192, 192));

SpatialRaster<Gray8BitColor> raster = circle.Rasterize(
    width: 0f,
    edgeFalloff: 0.25f,
    color: new Gray8BitColor(byte.MaxValue),
    rasterGeometry: grid);
raster.SaveAsPng("circle-distance.png");
```

![Circle distance raster](../../../assets/spatial2d/curves/circle-distance.png)

```csharp
var circle = new Circle(
    center: new PointXY(1f, 1f),
    radius: 2f);

CurveProjection projection = circle.Project(new PointXY(4f, 1f));

PointXY closestPoint = projection.ProjectedPoint; // (3, 1)
float distance = projection.Distance;             // 1
float circumference = circle.Length;              // 4 * PI
```

If the projected point is exactly at the circle center, projection uses the point on the positive X axis.
If the radius is zero, projection returns the center.
