# Handling an Unreachable Target

In this part of the tutorial, you will temporarily block the destination and handle the absence
of a route. A failed search is an expected result and is represented by `null`.

## Make the goal unreachable

Add this code after the successful search and its output:

```csharp
float originalGoalEntryCost = entryCosts[goal];
entryCosts[goal] = float.PositiveInfinity;

HexPath? blockedPath = transferCosts.FindShortestPath(start, goal);

Console.WriteLine(
    blockedPath is null
        ? "No route reaches the blocked goal."
        : "A route was found unexpectedly.");

entryCosts[goal] = originalGoalEntryCost;
```

Expected final line:

```text
No route reaches the blocked goal.
```

Every route must enter the destination from a neighbor. Giving `goal` an infinite entry cost
therefore makes it unreachable. The code restores the old cost so that the successful `path` and
the cost maps agree again before visualization.

Check for `null` before reading `HexIndexes` or `TotalCost`. If `start` and `goal` are equal, the
method instead returns a one-element path with zero cost, provided all cost values are valid.

Continue with [Visualizing the Route](visualizing-the-route.md).
