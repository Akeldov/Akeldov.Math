# Set Transfer Costs

Use <xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap> to describe the cost of each
directed step on a finite hex map. It combines two `IHexMap<float>` instances:

```text
cost(from → to) = ExitCosts[from] + EntryCosts[to]
```

This separation lets a cell cost one amount to enter and another amount to leave.

## Create entry and exit cost maps

The two maps must use the same topology. In this example, entering forest costs `4`, entering
any other terrain costs `1`, and all cells initially cost `0` to leave:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 3,
    height: 2,
    layout: Layout.OddR);

var terrain = new[]
{
    '.', 'F', '.',
    '.', '.', '.'
};

var exitCosts = new HexMap<float>(topology);
var entryCosts = new HexMap<float>(topology);

for (int index = 0; index < topology.Count; index++)
{
    entryCosts[index] = terrain[index] == 'F' ? 4f : 1f;
}

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
```

`HexMap<float>` initializes its cells to `0`, so the loop only needs to populate
`entryCosts`. You can instead fill both maps when the application has separate rules for entering
and leaving cells.

## Check the directed costs

The forest at `(1, 0)` and the plain at `(2, 0)` are neighbors. Calculate both directions to see
which cell supplies each part of the cost:

```csharp
var forest = new VectorXYInt(1, 0);
var plain = new VectorXYInt(2, 0);

Console.WriteLine($"Forest to plain: {transferCosts.GetTransferCost(forest, plain)}");
Console.WriteLine($"Plain to forest: {transferCosts.GetTransferCost(plain, forest)}");
```

The result is:

```text
Forest to plain: 1
Plain to forest: 4
```

The first step uses the plain's entry cost; the reverse step uses the forest's entry cost. A
route does not charge the entry cost of its starting cell or the exit cost of its destination,
because neither of those transitions occurs.

`GetTransferCost(from, to)` only adds the two stored values. It accepts any two in-bounds indices
and does not check whether they are adjacent. `FindShortestPath` is responsible for moving only
between edge-adjacent cells.

## Change costs at runtime

`HexTransferCostMap` retains the two source maps. Update either map when conditions change; the
next cost lookup or path search sees the new value:

```csharp
exitCosts[forest] = 2f;

Console.WriteLine($"Forest to plain: {transferCosts.GetTransferCost(forest, plain)}");
```

The result is now:

```text
Forest to plain: 3
```

The step combines the forest's exit cost of `2` with the plain's entry cost of `1`.

Use only non-negative finite values for ordinary traversable cells. `FindShortestPath` rejects
negative values, `float.NaN`, and negative infinity. To block movement with positive infinity,
continue with [Exclude Impassable Hexes](exclude-impassable-hexes.md). For the complete cost and
directionality contract, see [Pathfinding](../../concepts/spatial-algorithms/pathfinding.md).
