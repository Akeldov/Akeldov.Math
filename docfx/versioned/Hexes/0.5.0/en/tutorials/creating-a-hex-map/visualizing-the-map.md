# Visualizing the Map

In this part of the tutorial, you will render the map in the terminal. Odd rows in `OddR` start
with an extra space so that the character positions roughly follow the hex grid.

## Print the rows

Add this code after constructing the rings:

```csharp
Console.WriteLine();
Console.WriteLine("Legend: @ = center, 1 = neighbors, 2 = second ring");

for (int y = 0; y < topology.Resolution.Y; y++)
{
    if ((y & 1) == 1)
    {
        Console.Write(' ');
    }

    for (int x = 0; x < topology.Resolution.X; x++)
    {
        Console.Write($"{map[new VectorXYInt(x, y)]} ");
    }

    Console.WriteLine();
}
```

Expected output:

```text
Legend: @ = center, 1 = neighbors, 2 = second ring
. . 2 2 2 . .
 . 2 1 1 2 . .
. 2 1 @ 1 2 .
 . 2 1 1 2 . .
. . 2 2 2 . .
```

The `@` character is at `(3, 2)`. The six `1` characters are its immediate neighbors, while the
`2` characters are the visible part of the second ring inside the 7×5 map. A smaller map would
clip the ring at its boundaries.

Indenting odd rows is specific to the selected `OddR` layout. For `EvenR`, indent even rows; an
`OddQ` or `EvenQ` layout requires a representation that shifts columns instead.

You now have a finite hex map whose layout, coordinates, topology, and data agree. To produce a
pixel image with colors, continue with [Rasterizing a Hex Map](../rasterizing-a-hex-map/index.md).
