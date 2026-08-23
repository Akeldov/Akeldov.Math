# Rasterize a Signed-Distance Field

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

IRegion region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});

var grid = new RasterGeometry(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(160, 160));

var rasterizer = new SignedPointDistanceProviderRasterizer<Gray16BitColor>(distance =>
{
    float normalized = System.Math.Clamp((distance + 1f) / 2f, 0f, 1f);
    return new Gray16BitColor((ushort)(normalized * ushort.MaxValue));
});

SpatialRaster<Gray16BitColor> raster = region.Rasterize(grid, rasterizer);
raster.SaveAsPng("signed-distance.png");

static IContour CreateSquareContour(float left, float bottom, float right, float top)
{
    return new CompositeContour(new IContourPath[]
    {
        new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
        new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
        new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
        new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
    });
}
```
