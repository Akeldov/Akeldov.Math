# Poisson Disk Sampling

`PoissonDiskPointSampler` generates points in a rectangular 2D field while keeping a minimum distance between accepted samples.

## Distance Color Raster

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var fieldSize = new VectorXY(120f, 80f);
var grid = new RasterGrid(new PointXY(0f, 0f), fieldSize, new VectorXYInt(180, 120));

var sampler = new PoissonDiskPointSampler(new Random(45678), maxAttempts: 30);
var distanceField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 5f),
        new FloatPointInfluenceSource(1f, new PointXY(fieldSize.X, 0f), 13f)
    });

var samples = sampler.Sample(fieldSize, distanceField);

var raster = samples.Rasterize(grid, (sample, distance) =>
{
    float distanceT = MathF.Min(distance / sample.MinimalDistance, 1f);
    float distanceFill = (1f - distanceT) * 0.55f;
    float radiusT = MathF.Min(MathF.Max((sample.MinimalDistance - 5f) / 8f, 0f), 1f);

    float red = Blend(0.972f, Blend(0.125f, 0.961f, radiusT), distanceFill);
    float green = Blend(0.980f, Blend(0.510f, 0.620f, radiusT), distanceFill);
    float blue = Blend(0.988f, Blend(0.965f, 0.043f, radiusT), distanceFill);
    float pointAmount = MathF.Max(0f, 1f - distance / 1.15f);

    return new RGBA16BitColor(
        red: ToChannel(Blend(red, 0.058f, pointAmount)),
        green: ToChannel(Blend(green, 0.090f, pointAmount)),
        blue: ToChannel(Blend(blue, 0.165f, pointAmount)),
        alpha: ushort.MaxValue);
});

raster.SaveAsPng("poisson-disk-samples-rgba16.png");

static float Blend(float from, float to, float amount)
{
    return from * (1f - amount) + to * amount;
}

static ushort ToChannel(float value)
{
    return (ushort)MathF.Round(Math.Clamp(value, 0f, 1f) * ushort.MaxValue);
}
```

![Poisson disk samples rasterized with nearest-sample distance coloring](../../assets/spatial2d/poisson-disk/poisson-disk-samples-rgba16.png)

## Minimal Distance Rings

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var fieldSize = new VectorXY(120f, 80f);
var grid = new RasterGrid(new PointXY(0f, 0f), fieldSize, new VectorXYInt(180, 120));

var sampler = new PoissonDiskPointSampler(new Random(45678), maxAttempts: 30);
var distanceField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 5f),
        new FloatPointInfluenceSource(1f, new PointXY(fieldSize.X, 0f), 13f)
    });

var samples = sampler.Sample(fieldSize, distanceField);

var rasterizer = new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(
    pointRadius: 1.45f,
    ringThickness: 0.18f,
    backgroundGrayLevel: 0x1010,
    ringGrayLevel: 0x8a8a,
    pointGrayLevel: ushort.MaxValue);

var raster = samples.Rasterize(grid, rasterizer);

raster.SaveAsPng("poisson-disk-samples-rings-gray16.png");
```

![Poisson disk samples rendered with minimal-distance rings](../../assets/spatial2d/poisson-disk/poisson-disk-samples-rings-gray16.png)
