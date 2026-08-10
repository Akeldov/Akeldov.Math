# Sampling

Spatial sampling generates positions for procedural layouts, object placement, test data, and
partition sites. Spatial2D currently provides Poisson disk sampling for irregular point sets
whose members remain a controlled distance apart.

## Choose a sampling workflow

| Goal | Approach |
| --- | --- |
| Keep one distance between every pair of points | Call <xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler.Sample(Akeldov.Math.Spatial2D.VectorXY,System.Single)> with a constant finite positive distance. |
| Create dense and sparse areas in one point set | Supply an <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> whose finite positive values define the local spacing. |
| Reproduce a procedural layout | Construct the sampler with a seeded `Random` in a controlled environment. |
| Generate a different layout on each run | Supply an application-owned random source with varying state. |
| Inspect points and their exclusion distances | Rasterize the result with <xref:Akeldov.Math.Spatial2D.Rasterization.PoissonDiskPointSampleCollectionRingsGray16BitRasterizer>. |

<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> generates points
inside a half-open rectangle from `(0, 0)` inclusive to the requested size exclusive. Translate
the returned positions when the destination rectangle has another origin.

Each <xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSample> stores both
its position and the minimal distance requested there. The returned collection is a new mutable
list owned by the caller.

## Choose the parameters

| Parameter | Tradeoff |
| --- | --- |
| Field size | Defines the positive finite width and height of the sampled rectangle in world units. |
| Minimal distance | Larger values produce fewer, more widely spaced points. |
| `maxAttempts` | Higher values may fill gaps more densely but increase candidate-search work. |
| Random source | Controls the generated arrangement and whether it can be reproduced. |

With spatially varying spacing, the distance between any two accepted samples is at least the
greater of their two stored minimal distances. The distance field's `Min`, `Max`, and sampled
values must all be finite and positive.

## How-to guides

- [Generate Poisson disk points](generate-poisson-disk-points.md) — create constant-spacing and
  adaptive point sets, tune the sampler, visualize exclusion rings, and export the result as PNG.

## Related documentation

- [Spatial Algorithms](../../concepts/spatial-algorithms.md) explains Poisson disk invariants and
  compares sampling with Voronoi partitioning and influence culling.
- [Fields](../../concepts/fields.md) explains how to construct an `IFloatField` for adaptive
  spacing.
- [Rasterization](../../concepts/rasterization.md) describes raster geometry, color formats, and
  image export.
