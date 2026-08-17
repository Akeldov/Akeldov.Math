# Visualizing the Route

In this final part of the tutorial, you will overlay the successful route on the terrain map.
Odd rows receive one leading space to approximate the selected `OddR` layout in the terminal.

## Render the result

Add this code after restoring the goal's entry cost:

```csharp
var route = new HashSet<VectorXYInt>(path.HexIndexes);

Console.WriteLine();
Console.WriteLine("Legend: S = start, G = goal, * = route, F = forest, # = water");

for (int y = 0; y < topology.Resolution.Y; y++)
{
    if ((y & 1) == 1)
    {
        Console.Write(' ');
    }

    for (int x = 0; x < topology.Resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        char symbol = terrain[index] == 'W' ? '#' : terrain[index];

        if (route.Contains(index))
        {
            symbol = '*';
        }

        if (index == start)
        {
            symbol = 'S';
        }
        else if (index == goal)
        {
            symbol = 'G';
        }

        Console.Write($"{symbol} ");
    }

    Console.WriteLine();
}

Console.WriteLine($"Total cost: {path.TotalCost}");
```

The final section of the output is:

```text
Legend: S = start, G = goal, * = route, F = forest, # = water
. . . . . . .
 . . F F F . .
S * F # F . G
 . * F F F * .
. . * * * * .
Total cost: 8
```

The route avoids both the impassable water and the more expensive forest. The leading spaces are
only a console presentation detail; topology determines real adjacency.

You now have a complete terrain-aware pathfinding example. For the cost model, validation rules,
and one-way restrictions, continue with the [Pathfinding concept](../../concepts/spatial-algorithms/pathfinding.md).
