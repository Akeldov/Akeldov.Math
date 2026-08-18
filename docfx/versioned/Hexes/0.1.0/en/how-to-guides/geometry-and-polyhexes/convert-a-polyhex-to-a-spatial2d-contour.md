# Convert a Polyhex to a Spatial2D Contour

Use `ToRegion()` to build the exact Spatial2D boundary of the occupied hexes, then read the
region's `Contours` collection. The conversion removes shared edges between adjacent hexes and
returns closed `IContour` instances.

> [!NOTE]
> `GetContour()` is a topology operation: it returns a `Polyhex` mask containing unoccupied cells
> next to the source shape. It does not create a Spatial2D geometric contour.

## Build a boundary contour

Create a <xref:Akeldov.Math.Hexes.Geometry.PolyhexGeometry> so the mask has a physical hex radius,
import the contour extensions, and call `ToRegion()` with the layout used by the shape:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;

var geometry = new PolyhexGeometry(
    new bool[,]
    {
        { true, true  },
        { true, false }
    },
    radius: 2f);

ContourBasedRegion region = geometry.ToRegion(Layout.OddR);
IReadOnlyList<IContour> contours = region.Contours;

IContour contour = contours[0];

Console.WriteLine($"Contours: {contours.Count}");
Console.WriteLine($"Boundary length: {contour.Length}");
```

The result is:

```text
Contours: 1
Boundary length: 24
```

The example contains one connected component without holes, so indexing `contours[0]` is safe.
The contour is a closed chain of straight segments around the union of the three occupied hexes.
`ToRegion()` returns a new result and does not modify the geometry or its mask.

## Handle multiple contours

A polyhex with a hole or more than one disconnected occupied component produces multiple closed
contours. Keep the complete collection when the filled shape matters:

```csharp
foreach (IContour boundary in region.Contours)
{
    Console.WriteLine($"Boundary length: {boundary.Length}");
}
```

Do not rely on contour ordering or assume that the first contour is always the outer boundary.
The returned region uses the even-odd fill rule to interpret all boundaries together. Prefer
`region.Contains(point)`, `region.Distance(point)`, or `region.SignedDistance(point)` when you
need membership or distance for the complete polyhex rather than an individual boundary.

## Choose the layout consistently

The overload without a layout argument uses `Layout.OddR`. R layouts create pointy-top hexes and
Q layouts create flat-top hexes. `PolyhexGeometry` does not store the layout, so pass the same
orientation whenever you convert a mask.

There is no custom-origin argument. For R layouts, mask cell `[0, 0]` is centered at
`(HexApothem, HexRadius)`; for Q layouts, it is centered at `(HexRadius, HexApothem)`. Apply any
application-specific placement when composing the returned boundaries with other geometry.

The mask must contain at least one occupied cell. An empty mask has no geometric boundary, so
`ToRegion()` throws `InvalidOperationException`. A `null` geometry throws `ArgumentNullException`,
and an unsupported layout value throws `ArgumentOutOfRangeException`.

For the distinction between topology masks and spatial geometry, see
[Polyhexes](../../concepts/hex-grid-model/polyhexes.md). For the layout orientations and placement
formulas, see [Hex Grid Geometry](../../concepts/hex-grid-model/geometry.md).
