# Find Hex Neighbors

Use `GetAdjacents(layout)` to obtain the six hex indices that share an edge with a selected
`VectorXYInt`. Pass the topology's layout because neighbor offsets depend on its row or column
convention.

## Get the six neighbors

```csharp
using System.Collections.Generic;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);
var center = new VectorXYInt(0, 0);

VectorXYInt[] allNeighbors = center.GetAdjacents(topology.Layout);
```

`GetAdjacents` calculates adjacency on the infinite hex grid. Each call returns a new, mutable
array owned by the caller.

## Respect the map bounds

Neighbors of an edge cell can lie outside a finite map. Filter them before using them as
`HexMap<TValue>` indices:

```csharp
var inBoundsNeighbors = new List<VectorXYInt>();

foreach (VectorXYInt neighbor in allNeighbors)
{
    if (neighbor.X >= 0 && neighbor.X < topology.Resolution.X &&
        neighbor.Y >= 0 && neighbor.Y < topology.Resolution.Y)
    {
        inBoundsNeighbors.Add(neighbor);
    }
}
```

For the `OddR` corner `(0, 0)` in this example, the list contains `(1, 0)` and `(0, 1)`. The other
four indices exist on the infinite grid but lie outside the topology. `GetAdjacents` neither
clips nor wraps them; passing one to a map indexer throws `IndexOutOfRangeException`.

For larger distances, continue with
[Get a ring of a given radius](get-a-ring-of-a-given-radius.md). See
[Row and Column Indices](../../concepts/fundamentals/coordinate-systems/row-and-column-indices.md)
for layout-dependent adjacency and [Topology](../../concepts/hex-grid-model/topology.md) for
finite-map bounds.
