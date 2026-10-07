# Handle No Available Path

<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMapExtensions.FindShortestPath*> returns
`null` when no finite-cost route connects the source to the destination. Treat this as an expected
search result: check it before reading `HexIndexes` or `TotalCost`.

## Detect an unreachable destination

The middle cell of this one-column map has an infinite entry cost. Every route from `(0, 0)` to
`(0, 2)` would have to enter that cell, so no route is available:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 1,
    height: 3,
    layout: Layout.OddR);

var exitCosts = new HexMap<float>(topology, new[]
{
    1f,
    1f,
    1f
});

var entryCosts = new HexMap<float>(topology, new[]
{
    1f,
    float.PositiveInfinity,
    1f
});

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
var source = new VectorXYInt(0, 0);
var destination = new VectorXYInt(0, 2);

HexPath? path = transferCosts.FindShortestPath(source, destination);

if (path is null)
{
    Console.WriteLine("No route reaches the destination.");
}
else
{
    Console.WriteLine($"Route cost: {path.TotalCost}");
}
```

The result is:

```text
No route reaches the destination.
```

Keep the null check next to the search. In an application, this branch can also clear a previously
displayed route, disable a movement command, or ask the user to select another destination.

## Retry after the map changes

`HexTransferCostMap` retains its entry and exit maps. If a temporary obstacle disappears, update
the original map and search again; the transfer-cost object does not need to be reconstructed:

```csharp
var blocked = new VectorXYInt(0, 1);
entryCosts[blocked] = 1f;

HexPath? reopenedPath = transferCosts.FindShortestPath(source, destination);

Console.WriteLine($"Route found after reopening: {reopenedPath is not null}");
Console.WriteLine($"Route cost: {reopenedPath!.TotalCost}");
```

The result is now:

```text
Route found after reopening: True
Route cost: 4
```

Each of the two steps costs `1` to leave its source and `1` to enter its destination.

## Distinguish no route from invalid input

`null` means the request and cost maps were valid, but the search could not reach the destination.
Invalid input is reported with an exception instead:

| Condition | Result |
|---|---|
| No finite-cost route reaches the destination | `null` |
| `from` or `to` lies outside the topology | `ArgumentOutOfRangeException` |
| A cost is negative, `float.NaN`, or negative infinity | `InvalidOperationException` |
| The entry and exit maps have different topologies | `ArgumentException` from the `HexTransferCostMap` constructor |

Do not catch these exceptions as if they meant that a route was unavailable; correct the invalid
index, cost, or topology instead. Positive infinity is valid and deliberately marks an entry or
exit as impassable.

When the source and destination are equal, the result is not `null`: it contains the one index and
has a total cost of `0`, provided all stored costs are valid.

To create the initial search, see
[Find a Path Between Two Hexes](find-a-path-between-two-hexes.md). To control which transitions are
unavailable, see [Exclude Impassable Hexes](exclude-impassable-hexes.md). The complete behavior is
described in [Pathfinding](../../concepts/spatial-algorithms/pathfinding.md).
