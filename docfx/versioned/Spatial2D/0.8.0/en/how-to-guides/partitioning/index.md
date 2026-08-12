# Partitioning

Spatial2D partitioning assigns existing positioned objects to semantic groups. The current
partitioning API uses weighted Voronoi sites: each item is assigned to the site that wins the
weighted-distance comparison.

Use this section when you already have objects with positions—map cells, settlements, resources,
or sample points—and need to decide which site owns each object. The result contains groups of
items; it does not construct polygonal Voronoi boundaries.

## Choose the Workflow

| Goal | Approach |
| --- | --- |
| Assign every item to its nearest site | Give all sites the same positive weight. |
| Let selected sites claim a wider area | Increase those sites' weights. |
| Preserve one result for every configured site | Use `EmptyCellPolicy.LeaveAsIs`. |
| Return only populated partitions | Use `EmptyCellPolicy.Exclude`. |
| Reject a result containing an empty partition | Use `EmptyCellPolicy.ThrowException`. |
| Move sites toward the centers of their assigned items | Set `relaxationIterations` to a positive value. |
| Generate well-spaced initial site positions | Use Poisson disk sampling before partitioning. |

## Core Types

| Type | Purpose |
| --- | --- |
| <xref:Akeldov.Math.Spatial2D.IHasPosition2D> | Supplies the position of an item to partition. |
| <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.Site> | Defines a site position and non-negative weight. |
| <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.VoronoiItemPartitioner`1> | Assigns positioned items to sites and optionally performs centroid relaxation. |
| <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.VoronoiItemPartition`1> | Contains one resulting site and its assigned items. |
| <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.EmptyCellPolicy> | Controls how empty final partitions are handled. |

## Basic Pattern

Define an item type that implements `IHasPosition2D`, configure at least one weighted site, and
partition a non-empty item collection:

```csharp
var sites = new[]
{
    new Site(new PointXY(20f, 30f), weight: 1f),
    new Site(new PointXY(80f, 30f), weight: 1.5f)
};

var partitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> partitions =
    partitioner.Partition(items);
```

The returned list is a semantic result whose order and cardinality describe the partition model.
Each partition also exposes its assigned items as a copied read-only structural view.

## Important Distinction

`VoronoiItemPartitioner<TItem>` does not clip the plane or return curves, contours, or polygon
vertices. It compares each supplied item's position with the configured sites and places the item
in one bucket. To approximate visible regions, create a regular grid of positioned items and
render each item using the color of its resulting partition.

For an end-to-end example covering item definitions, weighted-distance rules, empty partitions,
relaxation, and validation, see
[Partition Items with Weighted Voronoi](partition-items-with-weighted-voronoi.md).

For a longer workflow that generates sites and visualizes a discrete map, follow the
[Procedural Space Partitioning tutorial](../../tutorials/procedural-space-partitioning/index.md).
