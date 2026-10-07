# Pathfinding

Akeldov.Math.Hexes finds a minimum-cost route through a finite rectangular hex map. The search
uses Dijkstra's algorithm, follows only edge-adjacent neighbors, and supports different costs in
opposite directions.

## The Hex Map as a Graph

<xref:Akeldov.Math.Hexes.HexMapTopology> defines the graph vertices and edges:

- each valid `VectorXYInt` is one vertex;
- an interior hex has at most six outgoing transitions;
- the layout determines the indices of those six neighbors;
- transitions outside the map resolution are not created.

Hex radius, origin, and physical distance between centers do not participate in the search. Two
maps with the same topology and costs produce the same result even when they are placed
differently in world space.

## Transfer Cost

<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap> combines two `IHexMap<float>`
implementations with matching topologies:

- `ExitCosts[from]` is the cost of leaving the source cell;
- `EntryCosts[to]` is the cost of entering the next cell.

One directed step is calculated as follows:

```text
cost(from → to) = ExitCosts[from] + EntryCosts[to]
```

Transitions `A → B` and `B → A` can therefore have different costs. A route does not charge the
entry cost of its first hex or the exit cost of its last hex; only transitions that are actually
taken contribute to the total.

`GetTransferCost(from, to)` returns this sum for any two indices inside the topology. The method
itself does not verify adjacency or add distance. `FindShortestPath` is the operation that limits
transitions to the six neighbors.

The transfer-cost object retains the supplied maps instead of taking snapshots. If a mutable
source map is changed, the next search observes the new values.

## Valid Values and Obstacles

Every finite cost must be non-negative. `float.NaN`, negative values, and
`float.NegativeInfinity` cause `InvalidOperationException`. Both maps are validated in full before
every search, including a search whose source and destination are equal.

`float.PositiveInfinity` is an allowed special value:

| Where infinity is stored | Effect |
|---|---|
| `EntryCosts[index]` | The hex cannot be entered from a neighbor |
| `ExitCosts[index]` | The hex cannot be left for a neighbor |
| Both maps | The hex is completely isolated from other cells |

This separation can model one-way restrictions. For example, an infinite entry cost does not
prevent a route from starting in that cell, while an infinite exit cost at the destination does
not prevent the route from ending there.

## Search Example

In this example, the direct route crosses an expensive upper cell. The algorithm chooses a longer
but cheaper detour through the lower row.

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

var costs = new HexTransferCostMap(exitCosts, entryCosts);

HexPath? path = costs.FindShortestPath(
    from: new VectorXYInt(0, 0),
    to: new VectorXYInt(2, 0));
```

The returned path visits `(0, 0)`, `(0, 1)`, `(1, 1)`, and `(2, 0)`. Each of its three steps
costs `1 + 1`, so `TotalCost` is `6`. The two-step route through the upper row would cost `202`.

## Search Result

`FindShortestPath` returns <xref:Akeldov.Math.Hexes.Pathfinding.HexPath> or `null`:

- `HexIndexes` is a read-only sequence from the source index through the destination;
- `TotalCost` is the sum of transitions between consecutive elements;
- `null` means that no route reaches the destination through finite-cost transitions.

When `from == to`, a successful result contains that single index and has zero cost. If several
routes have the same minimum cost, treat `TotalCost` as the guaranteed criterion and do not make
application behavior depend on one particular equivalent sequence.

Both `from` and `to` must be inside the topology. Mismatched cost-map topologies are rejected when
`HexTransferCostMap` is constructed, and out-of-map indices are rejected when the search starts.

Return to the [Spatial Algorithms overview](index.md), or continue to
[Space Partitioning](space-partitioning.md) when cells must be classified by world-space position
rather than connected by a route.
