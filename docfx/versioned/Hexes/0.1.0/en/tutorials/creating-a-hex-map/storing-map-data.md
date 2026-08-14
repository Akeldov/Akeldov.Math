# Storing Map Data

In this part of the tutorial, you will create a mutable map and write a character to every cell.
Characters are convenient for console visualization, but `HexMap<TValue>` can store values of any
type.

## Create and fill the map

Add a map after creating `topology`:

```csharp
var map = new HexMap<char>(topology);

for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        map[new VectorXYInt(x, y)] = '.';
    }
}
```

The constructor creates one value for each cell. The nested loops fill all 35 positions with a
period, which will represent an ordinary hex.

## Two indexers

Mark the center hex:

```csharp
var center = new VectorXYInt(3, 2);
map[center] = '@';

int centerFlatIndex = center.Y * topology.Resolution.X + center.X;

Console.WriteLine($"By XY index: {map[center]}");
Console.WriteLine($"By flat index {centerFlatIndex}: {map[centerFlatIndex]}");
```

Expected output:

```text
By XY index: @
By flat index 17: @
```

<xref:Akeldov.Math.Hexes.HexMap`1> provides a `VectorXYInt` indexer and a flat `int` indexer. Flat
storage uses row-major order: `X` changes first, and `(x, y)` maps to `y * width + x`. The
`VectorXYInt` indexer is generally clearer in application code.

Keep the `map` and `center` variables in `Program.cs` and continue with
[Finding Neighbors and Rings](finding-neighbors-and-rings.md).
