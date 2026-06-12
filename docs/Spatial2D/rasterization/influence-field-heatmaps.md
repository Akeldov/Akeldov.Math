# Influence Field Heatmaps

Influence field heatmap rasterizers convert sampled field values into color rasters.

Use heatmaps when inspecting interpolation, source culling, or procedural control maps.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f)
};

var field = new FloatPointInfluenceField(
    new NearestFloatInfluenceSampler<FloatPointInfluenceSource>(),
    sources);

var grid = new RasterGrid(
    new PointXY(0f, 0f),
    new VectorXY(100f, 64f),
    new VectorXYInt(160, 96));

var rasterizer = new FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer();
RGBA16BitRaster raster = field.Rasterize(grid, rasterizer);
```
