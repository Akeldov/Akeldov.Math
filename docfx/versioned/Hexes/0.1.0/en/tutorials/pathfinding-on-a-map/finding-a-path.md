# Finding a Path

In this part of the tutorial, you will search for the cheapest route from `start` to `goal`.
`FindShortestPath` follows only the six edge-adjacent neighbors and uses Dijkstra's algorithm.

## Run the search

Replace the water diagnostic at the end of `Program.cs` with this code:

```csharp
HexPath? path = transferCosts.FindShortestPath(start, goal);

if (path is null)
{
    Console.WriteLine("No route found.");
    return;
}

Console.WriteLine($"Total cost: {path.TotalCost}");
Console.WriteLine($"Hexes visited: {path.HexIndexes.Count}");

foreach (VectorXYInt index in path.HexIndexes)
{
    Console.WriteLine(index);
}
```

Expected output for Akeldov.Math.Hexes 0.1.0:

```text
Total cost: 8
Hexes visited: 9
(0, 2)
(1, 2)
(1, 3)
(2, 4)
(3, 4)
(4, 4)
(5, 4)
(5, 3)
(6, 2)
```

The direct route crosses forests and is more expensive. The returned route takes eight plain
steps around the lower edge, for a total cost of `8`. An equally cheap upper route exists, so
application logic should depend on the minimum `TotalCost`, not on which equivalent sequence is
chosen.

<xref:Akeldov.Math.Hexes.Pathfinding.HexPath.HexIndexes> includes both endpoints and exposes the
semantic result as a read-only sequence. `TotalCost` sums the transitions between consecutive
indexes.

Keep `path` in scope and continue with
[Handling an Unreachable Target](handling-an-unreachable-target.md).
