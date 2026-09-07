# Poisson Disk Sampler

<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> creates
irregular point sets with a controlled minimum spacing.

## Generate Poisson disk points

Poisson disk sampling fills a rectangular field while enforcing a minimum distance between
accepted samples. The result is irregular but avoids the tight clusters and large accidental
gaps common in independent uniform random sampling.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var sampler = new PoissonDiskPointSampler(
    random: new Random(12345),
    maxAttempts: 30);

var fieldSize = new VectorXY(100f, 60f);

List<PoissonDiskPointSample> samples = sampler.Sample(
    fieldSize,
    minimalDistance: 6f);

PointXY firstPoint = samples[0].Point;
float firstSpacing = samples[0].MinimalDistance;
```

Samples lie in the half-open rectangle from `(0, 0)` inclusive to `fieldSize` exclusive. The
sampler has no origin parameter; translate returned points when the target world rectangle starts
elsewhere.

`maxAttempts` limits the number of candidate points tried around each active sample. A larger
value can produce a denser result but performs more work. Passing a seeded `Random` makes runs
repeatable in a controlled environment, which is useful for tests and procedural generation.

The returned `List<PoissonDiskPointSample>` is new, mutable, and owned by the caller.

## Vary spacing with a field

The minimal distance can come from any `IFloatField`. This makes sparse and dense areas part of
the same sample set:

```csharp
using Akeldov.Math.Spatial2D.Fields;

var spacingField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(0f, 0f),
            value: 4f),
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(fieldSize.X, 0f),
            value: 12f)
    });

List<PoissonDiskPointSample> adaptiveSamples =
    sampler.Sample(fieldSize, spacingField);
```

The field's `Min` and `Max` and every sampled value must be finite and positive. Each accepted
sample stores the distance requested at its position. For every pair, the actual separation is
at least the greater of the two stored minimal distances, so a large-spacing sample cannot be
crowded by a small-spacing neighbor.

## See also

- [Samplers](index.md) — an overview of point-set generation.
- [Generate Poisson disk points](../../how-to-guides/sampling/generate-poisson-disk-points.md) —
  a practical guide, including visualization and PNG export.
- [Fields](../fields.md) — value sources for adaptive spacing.
- [Spatial Algorithms](../spatial-algorithms.md) — Voronoi partitioning and local neighborhoods.
