# Spatial Algorithms

Spatial2D provides algorithms for generating well-spaced point sets, assigning positioned items
to weighted sites, and selecting local influence neighborhoods. These algorithms complement the
[geometry model](geometry-model/index.md): they operate on positions and fields rather than
introducing new shape primitives.

## Choose an algorithm

Start from the result the application needs:

| Goal | Algorithm | Main type |
|---|---|---|
| Generate irregular points with controlled spacing | Poisson disk sampling | <xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> |
| Assign existing positioned items to weighted centers | Weighted Voronoi item partitioning | <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.VoronoiItemPartitioner`1> |
| Select the local triangle or hull feature around a query point | Delaunay source indexing | <xref:Akeldov.Math.Spatial2D.Fields.DelaunayInfluenceSourceIndex`1> |
| Exclude point sources hidden behind nearer half-plane boundaries | Half-plane source indexing | <xref:Akeldov.Math.Spatial2D.Fields.HalfPlaneInfluenceSourceIndex`1> |

Poisson disk sampling creates new positions. Voronoi partitioning groups objects that already
have positions. Influence source indexes own an immutable source snapshot and return a temporary
local subset for one field sample; they do not permanently repartition that snapshot.

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

## Partition items with weighted Voronoi sites

`VoronoiItemPartitioner<TItem>` assigns each `IHasPosition2D` item to one configured `Site`. This
is a semantic item partitioner: it returns item groups, not polygonal Voronoi cell geometry.

`PointXY` itself implements `IHasPosition2D`, so it can be partitioned directly:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

var sites = new[]
{
    new Site(new PointXY(0f, 0f), weight: 1f),
    new Site(new PointXY(10f, 0f), weight: 2f)
};

var items = new[]
{
    new PointXY(1f, 0f),
    new PointXY(4f, 0f),
    new PointXY(8f, 0f)
};

var partitioner = new VoronoiItemPartitioner<PointXY>(
    sites,
    EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<PointXY>> partitions =
    partitioner.Partition(items);
```

Away from an exact site position, finite positive-weight sites compete using squared distance
divided by squared weight. Increasing a site's weight therefore lets it claim items from farther
away.

Special weight cases are explicit:

- a site at the item's position wins before weighted-distance comparison;
- a zero-weight site can only receive coincident items;
- when positive-infinity sites exist, the nearest one wins for non-coincident items;
- at least one configured site must have positive weight.

Site and item positions must be finite. Item collections must be non-empty and contain no
`null` elements.

## Handle empty Voronoi partitions

Some sites may receive no items. <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.EmptyCellPolicy>
defines how the final result handles them:

| Policy | Behavior |
|---|---|
| `ThrowException` | Fails when any returned partition is empty; this is the default. |
| `Exclude` | Removes empty partitions from the semantic result. |
| `LeaveAsIs` | Preserves one partition per configured site, including empty ones. |

Use `LeaveAsIs` when result indices must remain aligned with site indices. Use `Exclude` when
only populated groups matter. Use `ThrowException` when every downstream partition must contain
at least one item.

The returned partition list is read-only because its cardinality and site association are part
of the algorithm result. Each `VoronoiItemPartition<TItem>.Items` collection is also a copied,
read-only structural view.

## Relax sites toward item centroids

The three-argument partitioner constructor accepts `relaxationIterations`. After each assignment,
every populated site moves to the centroid of its items, retaining its weight, and the items are
partitioned again. Empty sites keep their previous positions.

```csharp
var relaxedPartitioner = new VoronoiItemPartitioner<PointXY>(
    sites,
    relaxationIterations: 2,
    emptyCellPolicy: EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<PointXY>> relaxed =
    relaxedPartitioner.Partition(items);
```

Relaxation balances sites around the supplied discrete items; it does not compute centroids of
continuous polygonal cells. The sites exposed by the final partitions may therefore differ from
the original configured positions.


## Select local field neighborhoods

Influence source indexes solve a different problem from Voronoi partitioning: they own an
immutable snapshot of point sources and, for each query point, return the sources that a sampler
should consider.

`DelaunayInfluenceSourceIndex<TPointSource>` requires at least three sources with unique
positions. It builds a triangulation for non-collinear input. A query inside the hull receives the
three sources of its containing triangle; a query outside receives the nearest hull vertex or the
two endpoints of the nearest hull edge. Collinear inputs use an explicit fallback.

`HalfPlaneInfluenceSourceIndex<TPointSource>` accepts one or more sources. It processes them from
nearest to farthest and removes sources hidden behind perpendicular half-plane boundaries created
by those already selected.

Both indexes expose their retained snapshot through the read-only `Sources` property. Every
selection is a new mutable, non-empty list owned by the caller. Pass the index itself to a point
influence field; see [Fields](fields.md) for the full sampling pipeline.

## Compose algorithms into a workflow

The algorithms can feed one another without becoming coupled:

1. Generate candidate sites with Poisson disk sampling.
2. Attach weights or field values to selected positions.
3. Partition existing objects among the sites with weighted Voronoi assignment.
4. Sample or rasterize the result for visualization and downstream processing.

Keep every linear quantity in the same world coordinate unit. Use fixed random seeds when a
procedural pipeline must be reproducible, and choose empty-cell behavior explicitly when later
steps depend on result cardinality.

For practical examples, see:

- [Generate Poisson disk points](../how-to-guides/sampling/generate-poisson-disk-points.md)
- [Partition items with weighted Voronoi](../how-to-guides/partitioning/partition-items-with-weighted-voronoi.md)
- [Procedural space partitioning tutorial](../tutorials/procedural-space-partitioning/index.md)
- [Rasterization](rasterization.md)


