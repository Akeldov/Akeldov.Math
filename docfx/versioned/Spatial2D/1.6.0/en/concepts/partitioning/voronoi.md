# Voronoi Partitioning

<xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.VoronoiItemPartitioner`1> assigns existing
positioned objects to weighted sites.

## Partition items with weighted Voronoi sites

`VoronoiItemPartitioner<TItem>` assigns each `IHasPosition2D` item to one configured `Site`. This
is a semantic item partitioner: it returns item groups, not polygonal Voronoi cell geometry.

`PointXY` itself implements `IHasPosition2D`, so it can be partitioned directly:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
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

![On the x-axis, site A at 0 owns the item at 1. Site B at 10 owns the items at 4 and 8.](~/assets/spatial2d/voronoi/weighted-item-assignment.svg)

Diamonds mark the sites, and each arc connects an item to its assigned site. The item at
`(4, 0)` is closer to A, but B wins after weighting. Its score is `6² / 2² = 9`,
compared with `4² / 1² = 16` for A.

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
of the algorithm result. Each `VoronoiItemPartition<TItem>.Items` collection is a read-only view of the items assigned to its site.

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

## Build continuous cell regions

See [Partition a rectangle into Voronoi regions](voronoi-regions.md) for illustrated examples of curved boundaries, holes, and disconnected cells.

Use the same weighted sites with a bounded rectangle to obtain the region of each cell. With the default `LeaveAsIs` policy the result follows input order. An empty cell has a null `Region` and calling `GetCentroid()` on it throws. The optional relaxation step uses the area of the clipped region, including its components and holes.

```csharp
var bounds = new Akeldov.Math.Spatial2D.Regions.Rectangle(new PointXY(0f, 0f), new PointXY(12f, 8f));
var cells = bounds.PartitionByVoronoi(sites, relaxationIterations: 2);
foreach (var cell in cells)
{
    if (cell.Region != null)
    {
        PointXY centroid = cell.GetCentroid();
    }
}
```

## See also

- [Partitioning](index.md) — an overview of grouping positioned objects.
- [Partition items with weighted Voronoi](../../how-to-guides/partitioning/partition-items-with-weighted-voronoi.md) —
  a practical guide including input validation.
- [Procedural space partitioning](../../tutorials/procedural-space-partitioning/index.md) —
  a tutorial on generating sites and visualizing a discrete map.
- [Poisson Disk Sampler](../samplers/poisson-disk.md) — generating initial site positions.
