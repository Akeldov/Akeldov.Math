# Partition Items with Weighted Voronoi

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

var sites = new[]
{
    new Site(new PointXY(25f, 30f), weight: 1f),
    new Site(new PointXY(95f, 30f), weight: 2f)
};

var partitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> partitions = partitioner.Partition(items);
```

Voronoi partitions are semantic results. Preserve their order and cardinality unless your workflow intentionally changes the partition model.
