# Creating Voronoi Cells

`VoronoiItemPartitioner<TItem>` partitions positioned items rather than returning polygon
boundaries. For this map, each item represents the center of one square grid cell.

Add a list containing a 120-by-80 grid:

```csharp
var mapCells = new List<MapCell>(120 * 80);

for (int y = 0; y < 80; y++)
{
    for (int x = 0; x < 120; x++)
        mapCells.Add(new MapCell(x, y));
}
```

Add the `MapCell` type after the top-level statements at the end of `Program.cs`:

```csharp
sealed class MapCell : IHasPosition2D
{
    public MapCell(int x, int y)
    {
        X = x;
        Y = y;
        Position = new PointXY(x + 0.5f, y + 0.5f);
    }

    public int X { get; }
    public int Y { get; }
    public PointXY Position { get; }
}
```

Now assign the cells to their nearest sites:

```csharp
var partitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> partitions =
    partitioner.Partition(mapCells);

foreach (var partition in partitions)
{
    Console.WriteLine(
        $"Site {partition.Site.Position}: {partition.Items.Count} cells");
}
```

`MapCell` implements <xref:Akeldov.Math.Spatial2D.IHasPosition2D>, which is the only spatial
contract required by the partitioner. With equal site weights, each cell goes to the nearest site.

`LeaveAsIs` preserves one result partition per configured site even if a site receives no cells.
The returned list is a semantic result with stable ordering and cardinality, and each partition's
`Items` collection is a read-only structural view.

Continue with [Adding Weights](adding-weights.md).
