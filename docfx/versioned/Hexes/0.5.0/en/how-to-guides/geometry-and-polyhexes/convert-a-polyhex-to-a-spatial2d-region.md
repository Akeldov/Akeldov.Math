# Convert a Polyhex to a Spatial2D Region

Use `ToRegion()` when occupied cells must become one queryable Spatial2D shape. The conversion
builds the exact union of the regular hexes, removes shared internal edges, and preserves holes
and disconnected components through the even-odd fill rule.

## Create a region with a hole

A topology-only `Polyhex` has no physical cell size. Wrap its mask in
<xref:Akeldov.Math.Hexes.Geometry.PolyhexGeometry>, import the contour extensions, and pass the
layout that determines the hex orientation:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

const float HexRadius = 2f;
const Layout layout = Layout.OddR;

var geometry = new PolyhexGeometry(
    new bool[,]
    {
        { true, true,  true },
        { true, false, true },
        { true, true,  true }
    },
    radius: HexRadius);

ContourBasedRegion region = geometry.ToRegion(layout);

Console.WriteLine($"Contours: {region.Contours.Count}");
Console.WriteLine($"Fill rule: {region.FillRule}");
```

The first array dimension is Q and the second is R. The false cell at `[1, 1]` is surrounded by
occupied cells, so the result has an outer contour and a hole contour:

```text
Contours: 2
Fill rule: EvenOdd
```

`ToRegion()` creates a new <xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion>. It does not
change `geometry` or its immutable polyhex mask.

## Query the filled shape

Use the region rather than an individual contour when holes and multiple components must be
interpreted together. Add the following code after creating `region`:

```csharp
PointXY occupiedCenter = (PointXY)Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(
    q: 0,
    r: 0,
    hexRadius: HexRadius,
    layout: layout);
PointXY holeCenter = (PointXY)Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(
    q: 1,
    r: 1,
    hexRadius: HexRadius,
    layout: layout);
var outside = new PointXY(-1f, -1f);

Console.WriteLine($"Occupied center: {region.Contains(occupiedCenter)}");
Console.WriteLine($"Hole center: {region.Contains(holeCenter)}");
Console.WriteLine($"Outside: {region.Contains(outside)}");
```

The result is:

```text
Occupied center: True
Hole center: False
Outside: False
```

`Contains(point)` includes the region boundary. `Distance(point)` returns the non-negative
distance to the closest boundary. `SignedDistance(point)` is negative in the filled area, zero on
the boundary, and positive both outside the outer contour and inside the hole.

## Keep layout and placement consistent

The overload without a layout argument uses `Layout.OddR`. R layouts create pointy-top hexes and
Q layouts create flat-top hexes. `PolyhexGeometry` stores the radius but not the layout, so pass
the same layout whenever the mask is converted or its cell centers are calculated.

Conversion uses the library's default zero-hex placement and has no custom-origin argument. For R
layouts, mask cell `[0, 0]` is centered at `(HexApothem, HexRadius)`; for Q layouts, it is centered
at `(HexRadius, HexApothem)`. Transform or compose the resulting geometry separately when an
application-specific origin is required.

## Handle invalid input

The mask must contain at least one occupied cell. `ToRegion()` throws
`InvalidOperationException` for an empty polyhex because no closed boundary can be constructed.
A `null` geometry throws `ArgumentNullException`, and an unsupported layout value throws
`ArgumentOutOfRangeException`.

Use `ToApothemOffsetRegion()` instead when the filled boundary must be expanded outward by one
hex apothem. To work with the individual closed boundaries rather than the filled shape, see
[Convert a Polyhex to a Spatial2D Contour](convert-a-polyhex-to-a-spatial2d-contour.md). The
[Polyhexes concept](../../concepts/hex-grid-model/polyhexes.md) describes mask ownership,
placement, holes, and disconnected components in detail.
