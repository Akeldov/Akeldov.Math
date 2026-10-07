# Assigning Transfer Costs

In this part of the tutorial, you will turn terrain into directed transfer costs. One step costs
the exit value of the current hex plus the entry value of its neighbor.

## Create the cost maps

Replace the `Console.WriteLine` at the end of `Program.cs` with this code:

```csharp
static float GetEntryCost(char terrain) => terrain switch
{
    '.' => 1f,
    'F' => 4f,
    'W' => 1f,
    _ => throw new ArgumentOutOfRangeException(nameof(terrain))
};

var exitCosts = new HexMap<float>(topology);
var entryCosts = new HexMap<float>(topology);

for (int index = 0; index < topology.Count; index++)
{
    exitCosts[index] = 0f;
    entryCosts[index] = GetEntryCost(terrain[index]);
}

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);

float plainStep = transferCosts.GetTransferCost(
    new VectorXYInt(0, 0),
    new VectorXYInt(1, 0));
float forestStep = transferCosts.GetTransferCost(
    new VectorXYInt(1, 1),
    new VectorXYInt(2, 1));

Console.WriteLine($"Enter plain: {plainStep}");
Console.WriteLine($"Enter forest: {forestStep}");
```

Expected output:

```text
Enter plain: 1
Enter forest: 4
```

All exit costs are zero, so a transition is priced entirely by the terrain being entered. This
makes the route total easy to interpret: each visited plain after the starting hex adds `1`, and
each visited forest adds `4`. The starting hex itself is not charged because no transition enters
it.

<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap> retains both mutable maps. Changes made
to `entryCosts` or `exitCosts` are therefore visible to the next search.

Water is still assigned a temporary finite value. Continue with
[Adding Impassable Hexes](adding-impassable-hexes.md) to turn it into an obstacle.
