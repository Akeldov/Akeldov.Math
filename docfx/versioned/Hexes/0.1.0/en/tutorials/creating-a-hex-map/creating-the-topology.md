# Creating the Topology

In this part of the tutorial, you will define the finite rectangular domain of the hex map. A
topology combines the storage dimensions and the selected layout, but does not yet contain cell
values.

## A 7×5 map

Create a <xref:Akeldov.Math.Hexes.HexMapTopology> after the `layout` declaration:

```csharp
var topology = new HexMapTopology(
    width: 7,
    height: 5,
    layout: layout);

Console.WriteLine(
    $"Resolution: {topology.Resolution.X} × {topology.Resolution.Y}");
Console.WriteLine($"Hex count: {topology.Count}");
Console.WriteLine($"Topology layout: {topology.Layout}");
```

Expected output:

```text
Resolution: 7 × 5
Hex count: 35
Topology layout: OddR
```

`Resolution.X` is the number of columns, `Resolution.Y` is the number of rows, and `Count` is their
product. Valid indexes range from `0..6` on `X` and `0..4` on `Y`.

The topology describes only which cells exist and how they are laid out. It does not define the
hex radius, the grid's world-space placement, or the values stored in cells. The next step creates
a map that uses this topology for storage.

Keep the `topology` variable in `Program.cs` and continue with
[Storing Map Data](storing-map-data.md).
