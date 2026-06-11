# Rasterization

Rasterization samples geometry on a rectangular grid.
The core types live in the `Akeldov.Math.Spatial2D.Rasterization` namespace.
Grayscale rasters and image export helpers live in `Akeldov.Math.Spatial2D.Imaging`.

## Raster Grid

`RasterGrid` maps integer raster cells to world-space sample points.
The grid origin is the lower-left corner, and each cell is sampled at its center.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGrid(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(160, 160));

PointXY center = grid.GetCellCenter(0, 0);
```

## Signed Distance Raster

Signed-distance rasterizers convert distance to a grayscale value.
Negative distances are inside the contour or region, and positive distances are outside.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

var region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});

var grid = new RasterGrid(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(160, 160));

var rasterizer = new RegionSignedDistanceGray8BitRasterizer(distance =>
    distance <= 0f ? byte.MaxValue : byte.MinValue);

var raster = region.Rasterize(grid, rasterizer);
raster.SaveAsBmp("region-mask.bmp");

static Contour CreateSquareContour(float left, float bottom, float right, float top)
{
    return new Contour(new IFinitePath[]
    {
        new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
        new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
        new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
        new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
    });
}
```

Use `RegionSignedDistanceGray16BitRasterizer` with `SaveAsPng` when 16-bit grayscale output is needed.
