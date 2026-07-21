# Pathfinding

Hexes provides weighted shortest-path search over rectangular hex maps. The search follows the six edge-adjacent cells defined by the map layout and uses Dijkstra's algorithm.

## Transfer Costs

`HexTransferCostMap` combines two `IHexMap<float>` instances with matching topologies:

- the source cell contributes its exit cost;
- the destination cell contributes its entry cost;
- the cost of one step is the sum of those two values.

Finite costs must be non-negative. Positive infinity marks an entry or exit as impassable.

## Finding a Path

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(width: 3, height: 2, layout: Layout.OddR);
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

var costs = new HexTransferCostMap(exitCosts, entryCosts);
HexPath? path = costs.FindShortestPath(
    from: new VectorXYInt(0, 0),
    to: new VectorXYInt(2, 0));
```

`FindShortestPath` returns `null` when no finite-cost route exists. Otherwise, `HexPath.HexIndexes` contains a read-only sequence from source through destination, and `HexPath.TotalCost` contains the sum of all step costs. A search from a cell to itself returns that one cell with zero total cost.
