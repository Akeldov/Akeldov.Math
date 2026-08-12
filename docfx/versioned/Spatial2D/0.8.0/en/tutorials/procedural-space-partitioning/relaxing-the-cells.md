# Relaxing the Cells

Poisson disk sampling already distributes sites evenly, but the corresponding cells can still
have noticeably different shapes. Centroid relaxation moves each site toward the average position
of the items assigned to it and partitions the items again.

Replace the partitioner construction with:

```csharp
var partitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    relaxationIterations: 2,
    emptyCellPolicy: EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> partitions =
    partitioner.Partition(mapCells);
```

The partitioner performs two relaxation passes before returning the final result. Each pass:

1. assigns every map cell to a weighted site;
2. computes the centroid of each partition's items;
3. creates a site at that centroid with the original weight;
4. partitions the original map cells again.

An empty partition keeps its previous site position. Site weights are preserved throughout the
process, so heavier sites remain heavier after relaxation.

Relaxation uses the discrete `mapCells` positions. Increasing the grid resolution makes centroids
more precise but also increases the number of distance comparisons. Two iterations are a useful
starting point; more iterations cost more and do not necessarily improve the intended style.

Continue with [Visualizing the Result](visualizing-the-result.md).
