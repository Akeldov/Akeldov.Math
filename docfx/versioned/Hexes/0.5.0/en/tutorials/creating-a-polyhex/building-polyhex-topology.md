# Building Polyhex Topology

In this part of the tutorial, you will copy the mask into an immutable
<xref:Akeldov.Math.Hexes.Topology.Polyhex> and inspect its topology.

## Create the polyhex

Replace the `Console.WriteLine` after the mask with this code:

```csharp
var polyhex = new Polyhex(mask);

Console.WriteLine(
    $"Mask: {polyhex.QRSResolution.Q} x {polyhex.QRSResolution.R}");
Console.WriteLine($"Occupied hexes: {polyhex.HexCount}");
Console.WriteLine($"Hole occupied: {polyhex[2, 2]}");
```

Expected output:

```text
Mask: 4 x 4
Occupied hexes: 11
Hole occupied: False
```

`QRSResolution.Q` and `.R` preserve the two mask extents. `HexCount` counts only elements equal
to `true`, not all 16 positions.

The constructor copies the Boolean array. Later changes to `mask` do not alter `polyhex`, so the
polyhex can be shared safely as an immutable value. Empty shapes, holes, and disconnected
components are valid; the type does not force all occupied cells to be connected.

Keep `polyhex` in `Program.cs` and continue with
[Obtaining Edges and Vertices](obtaining-edges-and-vertices.md).
