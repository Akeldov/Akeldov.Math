# Poisson Disk Sampler

<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> generates points
in a rectangle with a minimum distance between neighbors. You choose the spacing, and the
number of points depends on which candidates the sampler accepts.

## Use a constant spacing

This example places points at least 9 world units apart in a rectangle 120 units wide and 80 units high.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var fieldSize = new VectorXY(120f, 80f);
var sampler = new PoissonDiskPointSampler(new Random(12345), maxAttempts: 30);
List<PoissonDiskPointSample> samples = sampler.Sample(fieldSize, minimalDistance: 9f);
```

![Poisson disk samples with equal-radius circles around their centers](~/assets/spatial2d/samplers/poisson-constant-distance.svg)

Each dot marks a sample’s `Point`. The gray circle around it has radius `MinimalDistance`.
The circles can overlap, but each center stays at least 9 units from every other center.

Both dimensions of `fieldSize` and the spacing must be finite and positive. Points lie in
`0 ≤ X < 120` and `0 ≤ Y < 80`. To place them elsewhere in the scene, add the desired origin
to each point. Each call returns a new, mutable `List<PoissonDiskPointSample>` owned by the caller.

`maxAttempts` limits how many candidates the sampler tries around an active point before
retiring it if none fit. A higher limit can fill more gaps at the cost of additional work.

## Vary spacing with a field

Pass an <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> to vary the spacing by position.
Here the minimum distance grows from 5 units at the left edge to 13 at the right.
Start a new sampler with the same seed to compare the two patterns.

```csharp
var spacingField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(weight: 1f, position: new PointXY(0f, 0f), value: 5f),
        new FloatPointInfluenceSource(weight: 1f, position: new PointXY(fieldSize.X, 0f), value: 13f)
    });

var adaptiveSampler = new PoissonDiskPointSampler(new Random(12345), maxAttempts: 30);
List<PoissonDiskPointSample> adaptiveSamples = adaptiveSampler.Sample(fieldSize, spacingField);
```

![Samples are denser on the left, with increasingly large spacing circles toward the right](~/assets/spatial2d/samplers/poisson-variable-distance.svg)

For each pair of points, the minimum separation is the larger of their two `MinimalDistance`
values. The field must report finite, positive bounds with `Min ≤ Max` and return values
within that range.

## See also

- [Samplers](index.md) — an overview of point-set generation.
- [Generate Poisson disk points](../../how-to-guides/sampling/generate-poisson-disk-points.md) —
  a practical guide, including visualization and PNG export.
- [Fields](../fields.md) — value sources for adaptive spacing.
- [Voronoi Item Partitioning](../partitioning/voronoi.md) — assigning objects to generated sites.
- [Procedural space partitioning](../../tutorials/procedural-space-partitioning/index.md) —
  combining site generation, partitioning, and visualization.
