# Adding Impassable Hexes

In this part of the tutorial, you will prevent routes from entering or leaving water hexes.
Akeldov.Math.Hexes represents a blocked transition with `float.PositiveInfinity`.

## Block the water

Replace the two diagnostic `Console.WriteLine` calls at the end of `Program.cs` with this loop:

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

float waterStep = transferCosts.GetTransferCost(
    new VectorXYInt(3, 1),
    new VectorXYInt(3, 2));

Console.WriteLine(
    $"Water is impassable: {float.IsPositiveInfinity(waterStep)}");
```

Expected output:

```text
Water is impassable: True
```

The cost maps were changed after `transferCosts` was constructed. The result still changes
because `HexTransferCostMap` retains the maps rather than copying their values.

An infinite entry cost prevents a route from entering a hex, while an infinite exit cost prevents
it from leaving. Setting both completely isolates water. Positive infinity is supported by the
pathfinder; negative values, `float.NaN`, and negative infinity are rejected.

Keep the loop and remove the diagnostic calculation when you continue with
[Finding a Path](finding-a-path.md).
