# Find the Nearest Hex to a Point

Use `PointXY.ToXYIndex` to identify the index of the hex that contains a spatial point. Pass the
radius, zero hex center, and complete layout of the same grid.

## Determine the index

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
const float hexRadius = 2f;
var zeroHexCenter = new VectorXY(10f, 20f);
var point = new PointXY(20.64f, 25.9f);

VectorXYInt index = point.ToXYIndex(
    hexRadius,
    zeroHexCenter,
    layout);

Console.WriteLine($"Index: ({index.X}, {index.Y})");
```

The result is:

```text
Index: (3, 2)
```

The method first converts the point to fractional QRS coordinates, selects the nearest integer
cell, and converts it to an index in the requested layout.

## Check map bounds

The result belongs to an unbounded hex grid. If the point must lie in a finite map, check the
index before accessing a `HexMap<T>`:

```csharp
var topology = new HexMapTopology(7, 5, layout);

bool isInside =
    index.X >= 0 && index.X < topology.Resolution.X &&
    index.Y >= 0 && index.Y < topology.Resolution.Y;
```

`hexRadius` must be finite and positive, and the point and center must have finite components.
Mixing an origin, radius, or layout from another grid produces a plausible but semantically
incorrect index.

A point exactly on a shared edge or vertex is assigned to one cell deterministically. Near such
a boundary, a small `float` change can select an adjacent hex, so do not use the result as a
stable identifier for a geometrically ambiguous point.

See also [Convert QRS to Spatial2D coordinates](convert-qrs-to-spatial2d-coordinates.md).
