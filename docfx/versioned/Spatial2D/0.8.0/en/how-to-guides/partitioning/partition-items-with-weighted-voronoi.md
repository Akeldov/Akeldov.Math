# Partition Items with Weighted Voronoi

Use weighted Voronoi partitioning to assign existing positioned items to semantic groups. Each
group belongs to a configured site; increasing a site's weight lets it claim items from farther
away. The result contains item buckets, not polygonal Voronoi cell geometry.

## Define positioned items

The item type must implement <xref:Akeldov.Math.Spatial2D.IHasPosition2D>. Its `Position` is the
point compared with the configured sites.

```csharp
using Akeldov.Math.Spatial2D;

public sealed class MapCell : IHasPosition2D
{
    public MapCell(string id, PointXY position)
    {
        Id = id;
        Position = position;
    }

    public string Id { get; }

    public PointXY Position { get; }
}
```

Create a non-empty collection of items with finite positions:

```csharp
var items = new[]
{
    new MapCell("west", new PointXY(20f, 30f)),
    new MapCell("center", new PointXY(50f, 30f)),
    new MapCell("east", new PointXY(80f, 30f)),
    new MapCell("far-east", new PointXY(105f, 30f))
};
```

## Configure weighted sites

A <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.Site> combines a finite position with a
non-negative weight. Equal weights reduce assignment to ordinary nearest-site comparison.

```csharp
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

var sites = new[]
{
    new Site(new PointXY(25f, 30f), weight: 1f),
    new Site(new PointXY(95f, 30f), weight: 2f)
};
```

Away from an exact site position, finite positive-weight sites compete using squared distance
divided by squared weight. The second site in this example can therefore claim the `center` item
even though that item is geometrically closer to the first site.

Use finite positive weights for ordinary weighted partitioning. The special cases are explicit:

- a site at the exact item position wins before weighted-distance comparison;
- a zero-weight site receives only coincident items;
- if positive-infinity sites exist, the nearest infinite-weight site receives every
  non-coincident item;
- at least one configured site must have positive weight.

## Partition the items

Choose an <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.EmptyCellPolicy> explicitly so the
result cardinality matches the downstream workflow:

```csharp
using System.Collections.Generic;

var partitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> partitions =
    partitioner.Partition(items);
```

With `LeaveAsIs`, the result contains one partition per configured site in site order. Read the
owning site through `Site` and the assigned objects through `Items`:

```csharp
VoronoiItemPartition<MapCell> westernPartition = partitions[0];
Site westernSite = westernPartition.Site;
IReadOnlyList<MapCell> westernItems = westernPartition.Items;
```

The partition list is read-only because its order, cardinality, and site associations are part of
the algorithm result. Each `Items` collection is also a copied, read-only structural view.

## Handle empty partitions

Some sites may receive no items. Select the policy according to what the consumer expects:

| Policy | Behavior |
| --- | --- |
| `ThrowException` | Throws when any final partition is empty. This is the default when no policy is supplied. |
| `Exclude` | Removes empty partitions; result indices no longer necessarily align with all configured sites. |
| `LeaveAsIs` | Preserves one partition per site, including empty partitions. |

Use `ThrowException` when every downstream group must contain data, `Exclude` when only populated
groups matter, and `LeaveAsIs` when site-to-result index alignment matters.

## Relax sites toward item centroids

Pass `relaxationIterations` when sites should move toward the centroids of their assigned items
before the final result is returned:

```csharp
var relaxedPartitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    relaxationIterations: 2,
    emptyCellPolicy: EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> relaxedPartitions =
    relaxedPartitioner.Partition(items);
```

Each populated site moves to the centroid of its assigned discrete items while retaining its
weight, then the items are assigned again. Empty sites keep their previous positions. This does
not calculate centroids of continuous polygonal cells, so the final partition sites may differ
from the originally configured positions.

## Validate inputs

The site collection and item collection must both be non-empty. Site and item positions must be
finite, items must not be `null`, site weights must be non-negative and not `NaN`, and at least
one site weight must be positive.

For the weighted-distance rules and ownership contracts in more depth, see
[Spatial Algorithms](../../concepts/spatial-algorithms.md). To generate well-spaced candidate
site positions first, see [Generate Poisson disk points](../sampling/generate-poisson-disk-points.md).
