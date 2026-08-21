# Exclude Impassable Hexes

Set an entry or exit cost to `float.PositiveInfinity` to prevent the pathfinder from using the
corresponding transition. <xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMapExtensions.FindShortestPath*>
skips every adjacent step whose combined transfer cost is positive infinity.

```text
cost(from → to) = ExitCosts[from] + EntryCosts[to]
```

## Block a hex completely

Set both costs to positive infinity when a hex must not be entered or left. The following map
blocks `(1, 0)`, so the shortest path from `(0, 0)` to `(2, 0)` takes the lower route:

```csharp
using System.Linq;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 3,
    height: 2,
    layout: Layout.OddR);

var exitCosts = new HexMap<float>(topology);
var entryCosts = new HexMap<float>(topology, new[]
{
    1f, 1f, 1f,
    1f, 1f, 1f
});

var blocked = new VectorXYInt(1, 0);
entryCosts[blocked] = float.PositiveInfinity;
exitCosts[blocked] = float.PositiveInfinity;

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
HexPath? path = transferCosts.FindShortestPath(
    new VectorXYInt(0, 0),
    new VectorXYInt(2, 0));

Console.WriteLine($"Path found: {path is not null}");
Console.WriteLine($"Path enters blocked hex: {path!.HexIndexes.Contains(blocked)}");
```

The result is:

```text
Path found: True
Path enters blocked hex: False
```

`HexTransferCostMap` retains the two cost maps, so changes made after its construction also affect
the next path search. Restore finite non-negative values when a temporarily blocked hex becomes
traversable again.

## Choose which direction to block

Entry and exit costs have different effects:

- `entryCosts[index] = float.PositiveInfinity` prevents every route from entering the hex. A
  route that starts there can still leave when its exit cost is finite.
- `exitCosts[index] = float.PositiveInfinity` prevents every route from leaving the hex. A route
  can still enter it when its entry cost is finite, so it can still be used as a destination.
- Setting both values to positive infinity isolates the hex in both directions.

This distinction can represent one-way endpoints as well as fully impassable terrain. Because a
step adds the source exit cost to the destination entry cost, either infinite component is enough
to block that directed step.

## Block a terrain type

When a terrain map identifies impassable cells, update both cost maps in one pass:

```csharp
for (int index = 0; index < topology.Count; index++)
{
    if (terrain[index] != 'W')
    {
        continue;
    }

    entryCosts[index] = float.PositiveInfinity;
    exitCosts[index] = float.PositiveInfinity;
}
```

If the blocked cells separate the source from the destination, `FindShortestPath` returns `null`.
Handle that result as shown in [Handle No Available Path](handle-no-available-path.md).

Positive infinity is the only non-finite value supported as a barrier. Negative costs,
`float.NaN`, and negative infinity cause `FindShortestPath` to throw an
`InvalidOperationException`. To configure ordinary traversal costs, see
[Set Transfer Costs](set-transfer-costs.md). For the complete pathfinding contract, see
[Pathfinding](../../concepts/spatial-algorithms/pathfinding.md).
