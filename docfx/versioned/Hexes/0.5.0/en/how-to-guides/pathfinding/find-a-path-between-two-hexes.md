# Find a Path Between Two Hexes

Call
<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMapExtensions.FindShortestPath*>
on a <xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap> to find a minimum-cost route between
two cells. The search follows edge-adjacent hexes and minimizes total transfer cost, which is not
necessarily the number of steps.

## Create a searchable map

The following `3 × 2` map makes the middle cell of the upper row expensive. Every ordinary step
costs `2`: `1` to leave its source and `1` to enter its destination.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 3,
    height: 2,
    layout: Layout.OddR);

var exitCosts = new HexMap<float>(topology, new[]
{
    1f, 100f, 1f,
    1f,   1f, 1f
});

var entryCosts = new HexMap<float>(topology, new[]
{
    1f, 100f, 1f,
    1f,   1f, 1f
});

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
```

Both cost maps must use the same topology. A directed step from `A` to `B` costs
`exitCosts[A] + entryCosts[B]`.

## Find and read the path

Pass in the source and destination indices. Both must lie inside the topology:

```csharp
var source = new VectorXYInt(0, 0);
var destination = new VectorXYInt(2, 0);

HexPath? path = transferCosts.FindShortestPath(source, destination);

if (path is null)
{
    Console.WriteLine("No path is available.");
    return;
}

Console.WriteLine($"Total cost: {path.TotalCost}");
foreach (VectorXYInt index in path.HexIndexes)
{
    Console.WriteLine($"({index.X}, {index.Y})");
}
```

The result is:

```text
Total cost: 6
(0, 0)
(0, 1)
(1, 1)
(2, 0)
```

<xref:Akeldov.Math.Hexes.Pathfinding.HexPath.HexIndexes> is a read-only sequence that includes both
the source and destination. <xref:Akeldov.Math.Hexes.Pathfinding.HexPath.TotalCost> is the sum of
the directed transfer costs between consecutive indices.

The direct upper route uses only two steps, but entering and leaving `(1, 0)` makes it cost `202`.
The pathfinder therefore selects the three-step lower route, whose total cost is `6`.

## Account for special results

- If no finite-cost route reaches the destination, `FindShortestPath` returns `null`.
- If the source and destination are equal, the result contains that one index and has a total
  cost of `0`.
- If several routes have the same minimum cost, do not depend on one particular sequence of
  indices; only the minimum total cost is guaranteed.

The cost maps are read again for each search. You can change terrain costs or obstacles and call
`FindShortestPath` again without rebuilding `HexTransferCostMap`.

To configure the maps used above, see [Set Transfer Costs](set-transfer-costs.md). To block cells,
see [Exclude Impassable Hexes](exclude-impassable-hexes.md). For an application-specific fallback
when the result is `null`, continue with [Handle No Available Path](handle-no-available-path.md).
The full algorithm and validation contract is described in
[Pathfinding](../../concepts/spatial-algorithms/pathfinding.md).
