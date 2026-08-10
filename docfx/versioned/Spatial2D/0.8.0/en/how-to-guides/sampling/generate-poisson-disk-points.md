# Generate Poisson Disk Points

Use Poisson disk sampling to generate an irregular point set without tight clusters. The sampler
fills a rectangular area while keeping accepted points at least a configured distance apart.

## Generate points with constant spacing

Create a <xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> with a
random-number generator and the maximum number of candidates to try around each active point.
Use a fixed seed when the result must be repeatable in a controlled environment.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var sampler = new PoissonDiskPointSampler(
    random: new Random(12345),
    maxAttempts: 30);

var fieldSize = new VectorXY(120f, 80f);

List<PoissonDiskPointSample> samples = sampler.Sample(
    fieldSize,
    minimalDistance: 9f);
```

Every <xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSample> contains its
`Point` and the `MinimalDistance` used when it was accepted. Points lie in the half-open rectangle
from `(0, 0)` inclusive to `fieldSize` exclusive.

The returned `List<PoissonDiskPointSample>` is new, mutable, and owned by the caller. You can
filter, append, transform, or reuse it without changing sampler state.

## Tune density and cost

The two main parameters have different effects:

| Parameter | Effect |
| --- | --- |
| `minimalDistance` | A larger value produces fewer, more widely separated points. It must be finite and positive. |
| `maxAttempts` | A larger value may fill remaining gaps more densely, but tries more candidates and performs more work. It must be positive. |

Use a seeded `Random` for tests, saved procedural worlds, and other cases that need controlled
repeatability. Use an application-owned random source when each run should produce a different
layout.

## Vary spacing across the area

Pass an <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> instead of a constant distance when some
parts of the area should be denser than others. This example varies spacing from `5` near the left
side to `13` near the right side:

```csharp
using Akeldov.Math.Spatial2D.Fields;

var spacingField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(0f, 0f),
            value: 5f),
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(fieldSize.X, 0f),
            value: 13f)
    });

List<PoissonDiskPointSample> adaptiveSamples =
    sampler.Sample(fieldSize, spacingField);
```

The field's `Min`, `Max`, and every sampled value must be finite and positive. Each accepted
sample stores the distance requested at its position. For every pair of accepted samples, their
actual separation is at least the greater of their two stored minimal distances.

## Visualize the result

The built-in ring rasterizer draws every accepted point and its minimal-distance circle. Use the
same world-space size for the raster geometry as for sampling:

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: fieldSize,
    resolution: new VectorXYInt(600, 400));

var rasterizer = new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(
    pointRadius: 1.2f,
    ringThickness: 0.2f,
    backgroundGrayLevel: Gray16BitColor.Black,
    ringGrayLevel: new Gray16BitColor(0x6000),
    pointGrayLevel: Gray16BitColor.White);

var raster = rasterizer.Rasterize(samples, geometry);
raster.SaveAsPng("poisson-disk-points.png");
```

`SaveAsPng` writes the image relative to the application's working directory.

## Use a different origin

The sampler always generates coordinates relative to `(0, 0)` and does not accept an origin.
When the destination rectangle starts elsewhere, add the destination offset to each sample point
when consuming or copying the result. Keep `MinimalDistance` unchanged because translation does
not alter spacing.

For the algorithm's invariants and its relationship to other spatial algorithms, see
[Spatial Algorithms](../../concepts/spatial-algorithms.md). See
[Rasterization](../../concepts/rasterization.md) for other ways to turn spatial data into images.
