# Converting to Spatial2D Geometry

In this part of the tutorial, you will associate the complete immutable mask with a physical hex
radius. <xref:Akeldov.Math.Hexes.Geometry.PolyhexGeometry> combines the polyhex topology with the
dimensions needed for Spatial2D conversion.

## Add the geometry wrapper

Add this code after constructing the sample edges:

```csharp
var geometry = new PolyhexGeometry(polyhex, hexRadius);

Console.WriteLine($"Geometry cells: {geometry.HexCount}");
Console.WriteLine($"Hex radius: {geometry.HexRadius}");
```

Expected final lines:

```text
Geometry cells: 11
Hex radius: 1
```

The wrapper retains the immutable `polyhex` and exposes its mask through the same read-only
indexers. It also stores `HexRadius` and derives `HexApothem` as
`HexRadius * sqrt(3) / 2`.

The layout is intentionally not stored in `PolyhexGeometry`. Choose the same layout whenever the
mask is converted to XY points, contours, or regions. The tutorial continues to use `OddR`.

The radius must be finite and greater than zero. It is expressed in the coordinate-space units
used by the resulting Spatial2D geometry.

Continue with [Creating a Region](creating-a-region.md).
