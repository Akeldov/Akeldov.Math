# Creating a Region

In this part of the tutorial, you will convert all occupied cells into one Spatial2D
<xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion>. Shared internal edges are omitted, while
the false cell surrounded by the shape remains a hole.

## Generate the boundary

Add these imports at the top of `Program.cs`:

```csharp
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Spatial2D.Regions;
```

Then add this code after constructing `geometry`:

```csharp
ContourBasedRegion region = geometry.ToRegion(layout);

Console.WriteLine($"Region contours: {region.Contours.Count}");
Console.WriteLine($"Fill rule: {region.FillRule}");
```

Expected final lines:

```text
Region contours: 2
Fill rule: EvenOdd
```

One closed contour bounds the outside of the occupied cells and another bounds the hole at
`[2, 2]`. The even-odd fill rule considers points inside both nested contours to be outside the
filled region.

`ToRegion` returns a new semantic result and does not modify `geometry` or `polyhex`. Shapes with
several disconnected components can also produce several contours. Do not rely on contour order;
use the region's fill and distance operations as the public geometric contract.

Continue with [Rasterizing the Result](rasterizing-the-result.md).
