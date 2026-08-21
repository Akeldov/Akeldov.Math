# Finding Neighbors and Rings

In this part of the tutorial, you will mark the six neighbors of the center hex and the cells at
distance two. Adjacency depends on the layout, while distance is simplest to calculate after
converting indexes to QRS coordinates.

## Immediate neighbors

Add the topology namespace at the top of `Program.cs`:

```csharp
using Akeldov.Math.Hexes.Topology;
```

After creating `map` and `center`, retrieve the adjacent indexes:

```csharp
foreach (VectorXYInt neighbor in center.GetAdjacents(topology.Layout))
{
    if (IsInside(neighbor, topology))
    {
        map[neighbor] = '1';
    }
}
```

`GetAdjacents` returns six neighbors on the infinite grid. For a hex near an edge, some indexes
can fall outside the finite map, so check them before using the map indexer:

```csharp
static bool IsInside(VectorXYInt index, HexMapTopology topology) =>
    index.X >= 0 &&
    index.X < topology.Resolution.X &&
    index.Y >= 0 &&
    index.Y < topology.Resolution.Y;
```

## The second ring

The package has no dedicated ring method. Iterate over the finite map and select indexes whose QRS
distance from the center is two:

```csharp
VectorQRSInt centerQrs = center.ToQRSIndex(topology.Layout);

for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        VectorQRSInt indexQrs = index.ToQRSIndex(topology.Layout);

        if (GetHexDistance(centerQrs, indexQrs) == 2)
        {
            map[index] = '2';
        }
    }
}
```

Add the distance function next to `IsInside`:

```csharp
static int GetHexDistance(VectorQRSInt first, VectorQRSInt second)
{
    int deltaQ = Math.Abs(first.Q - second.Q);
    int deltaR = Math.Abs(first.R - second.R);
    int deltaS = Math.Abs(first.S - second.S);

    return Math.Max(deltaQ, Math.Max(deltaR, deltaS));
}
```

The maximum absolute component difference is the number of steps between two hexes. The result is
independent of `OddR`, `EvenR`, `OddQ`, or `EvenQ`; the layout participates only in converting the
source `VectorXYInt` values.

Continue with [Visualizing the Map](visualizing-the-map.md) to see the marked rings.
