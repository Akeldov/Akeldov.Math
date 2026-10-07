# Choosing a Layout

In this part of the tutorial, you will select the placement rule for the rectangular hex map.
Continue working in the `HexMap.Tutorial` project created in the previous step.

## Orientation and offset

<xref:Akeldov.Math.Hexes.Layout> specifies both the hex orientation and which rows or columns are
shifted relative to their neighbors:

| Value | Orientation | Shifted axis |
| --- | --- | --- |
| `OddR` | pointy top | odd rows shift right |
| `EvenR` | pointy top | even rows shift right |
| `OddQ` | flat top | odd columns shift down |
| `EvenQ` | flat top | even columns shift down |

This tutorial uses `OddR`: `X` and `Y` read naturally as column and row, and odd rows will be
indented when the map is rendered.

Replace the initial check in `Program.cs` with the layout selection:

```csharp
using Akeldov.Math.Hexes;

Layout layout = Layout.OddR;

Console.WriteLine($"Layout:       {layout}");
Console.WriteLine($"Pointy top:   {layout.IsPointyTop()}");
Console.WriteLine($"Flat top:     {layout.IsFlatTop()}");
Console.WriteLine($"Orientation:  {layout.GetHexOrientation()}");
```

The program prints:

```text
Layout:       OddR
Pointy top:   True
Flat top:     False
Orientation:  PointyTop
```

Use the same layout for coordinate conversion, neighbor queries, topology creation, and
visualization. Otherwise, one index would identify different hexes at different stages.

Keep the `layout` variable in `Program.cs` and continue with
[Working with QRS Coordinates](working-with-qrs-coordinates.md).
