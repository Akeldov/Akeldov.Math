# Build an Influence Heatmap

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 50f)
};

var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);

var grid = new RasterGrid(
    new PointXY(0f, 0f),
    new VectorXY(100f, 64f),
    new VectorXYInt(160, 96));

var rasterizer = new FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer();
RGBA16BitRaster raster = field.Rasterize(grid, rasterizer);
raster.SaveAsPng("influence-heatmap.png");
```
