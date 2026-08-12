# Exporting the Image

Extensions in the `Akeldov.Math.Spatial2D.Imaging` namespace save color rasters as PNG files. Add
this code after rasterization:

```csharp
const string outputPath = "influence-heatmap.png";
raster.SaveAsPng(outputPath);

Console.WriteLine($"Map saved to: {Path.GetFullPath(outputPath)}");
```

Run the project:

```powershell
dotnet run
```

The `influence-heatmap.png` file appears in the current working directory. It uses 16 bits per
channel, matching the raster's `RGBA16BitColor` elements.

## Complete Code

The final `Program.cs` is:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 25f),
    new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 50f),
    new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 75f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 100f)
};

var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(100f, 70f),
    resolution: new VectorXYInt(800, 560));

SpatialRaster<RGBA16BitColor> raster = field.RasterizeHeatMap(grid);

const string outputPath = "influence-heatmap.png";
raster.SaveAsPng(outputPath);

Console.WriteLine($"Map saved to: {Path.GetFullPath(outputPath)}");
```

You now have a reproducible pipeline: sources → sampling → local culling → rasterization → PNG.
Change source values and positions, weights, sampling strategy, or grid resolution without
changing the other stages.

For more about field semantics and the available source types, see
[Influence Fields](../../concepts/fields.md).
