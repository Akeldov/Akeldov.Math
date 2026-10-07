# Coordinate Discretization

Discretization assigns a continuous coordinate to one hex cell. Akeldov.Math.Hexes exposes two
entry points: round fractional QRS coordinates to an integer QRS index, or classify a world-space
point directly into an offset-grid index. These operations are different from exact conversions
between two integer coordinate representations.

| Source and result | Operation | What it does |
| --- | --- | --- |
| <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> → <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> | `ToQRSIndex(layout)` | Selects the nearest hex. |
| `PointXY` → `VectorXYInt` | `ToXYIndex(radius, origin, layout)` | Selects the containing hex and returns its offset index. |
| `VectorQRSInt` ↔ `VectorXYInt` | `ToXYIndex(layout)` / `ToQRSIndex(layout)` | Changes the integer representation without selecting another cell. |
| <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> → <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> | Explicit cast | Truncates components; it is not nearest-hex rounding. |

See [QRS coordinates](coordinate-systems/qrs-coordinates.md),
[row-and-column indices](coordinate-systems/row-and-column-indices.md), and
[spatial coordinates](coordinate-systems/spatial-coordinates.md) for the roles of these types.

## Round Fractional QRS Coordinates

Call `ToQRSIndex(Layout)` when a fractional
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> must become one logical hex index. The method:

1. Derives `S` as `-Q - R`.
2. Rounds `Q`, `R`, and `S` individually with `MathF.Round` (midpoints round to even).
3. If the three rounded components no longer sum to zero, corrects the component with the
   greatest rounding error from the other two.
4. Returns a <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> whose
   `Q + R + S` is exactly zero.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

var fractional = new VectorQRS(1.2f, -2.3f); // S = 1.1

VectorQRSInt nearest = fractional.ToQRSIndex(Layout.OddR);
// nearest: Q = 1, R = -2, S = 1

VectorXYInt offset = nearest.ToXYIndex(Layout.OddR);
// offset: X = 0, Y = -2
```

The result is a QRS index; odd/even offset is not embedded in it. Convert the integer result with
the same <xref:Akeldov.Math.Hexes.Layout> only when an offset-storage index is needed.

### Ties and Orientation

Most coordinates have one nearest hex and produce the same QRS result for every layout. A point
exactly on a shared edge or vertex can be equally close to multiple cells, so the API needs a
deterministic tie rule. `OddR` and `EvenR` use the pointy-top correction order; `OddQ` and `EvenQ`
use the flat-top correction order. In particular, when `Q` and `R` share the largest error and
`S` has a smaller error, row layouts correct `R`, while column layouts correct `Q`. If a largest
error tie includes `S`, `S` is corrected.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;

var boundary = new VectorQRS(0.5f, 0.5f); // S = -1

VectorQRSInt pointyTop = boundary.ToQRSIndex(Layout.OddR);
// Q = 0, R = 1, S = -1

VectorQRSInt flatTop = boundary.ToQRSIndex(Layout.OddQ);
// Q = 1, R = 0, S = -1
```

The odd and even variants of one orientation always apply the same QRS rounding. They differ only
when that QRS index is represented as an offset row-and-column index. See
[Layouts](layouts.md) for the four offset conventions.

## Explicit Casts Truncate

An explicit cast to <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> converts `Q` and `R` to
integers by truncating them toward zero, then derives `S` from those two values. It does not round
each component and does not search for the nearest hex.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;

var source = new VectorQRS(1.9f, -2.9f);

VectorQRSInt truncated = (VectorQRSInt)source;
// Q = 1, R = -2, S = 1

VectorQRSInt nearest = source.ToQRSIndex(Layout.OddR);
// Q = 2, R = -3, S = 1
```

Use the cast only when component truncation is deliberately required. For cell selection, use
`ToQRSIndex`. In particular, do not use the cast as a shortcut for negative coordinates: values
such as `-0.9f` become zero, which biases the result toward the QRS origin.

## Classify a World-Space Point

`PointXY.ToXYIndex(hexRadius, hexFieldOrigin, layout)` finds the hex that contains a world-space
point and returns its `VectorXYInt` offset index. Conceptually it performs the following steps:

1. Subtracts `hexFieldOrigin` from the point.
2. Projects the shifted point onto the QRS axes selected by the layout orientation.
3. Divides by `hexRadius` to express the result in hex-radius units.
4. Applies the same orientation-aware cube rounding described above.
5. Converts the integer QRS result to the odd/even offset convention requested by `layout`.

For a shifted point `(x, y)`, the fractional components are:

| Layout orientation | Fractional QRS projection |
| --- | --- |
| `OddR`, `EvenR` (pointy-top) | `q = (x / sqrt(3) - y / 3) / radius`; `r = (2 * y / 3) / radius` |
| `OddQ`, `EvenQ` (flat-top) | `q = (2 * x / 3) / radius`; `r = (y / sqrt(3) - x / 3) / radius` |

`hexRadius` is the center-to-vertex distance in the same coordinate-space unit as the point and
origin. `hexFieldOrigin` is the world-space **center of the zero hex**, not a corner of the map.
Always pass the radius, origin, and layout used to construct or render the grid.

The following round trip starts at an offset index, obtains that cell's center, and classifies the
center back to the same index:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

var layout = Layout.OddR;
const float radius = 3f;
var origin = new VectorXY(10f, -20f);
var expected = new VectorXYInt(2, 3);

VectorXY center = expected.GetHexCenter(radius, origin, layout);
var point = new PointXY(center.X, center.Y);

VectorXYInt actual = point.ToXYIndex(radius, origin, layout);
// actual == expected

VectorQRSInt qrs = actual.ToQRSIndex(layout);
// Convert to QRS only if the following algorithm needs logical hex coordinates.
```

The overload `GetHexCenter(radius, layout)` uses a layout-dependent default origin. When its
result must be converted back, obtain the matching origin with
`VectorXYInt.Zero.GetHexCenter(radius, layout)` and pass that value to `ToXYIndex`. An arbitrary
`VectorXY.Zero` is not interchangeable with the default origin.

### Layout-Specific Helpers

When the layout is fixed by the code path, the following helpers perform the same classification
without taking a `Layout` argument:

| Helper | Equivalent general call |
| --- | --- |
| `ToOddRXYIndex(radius, origin)` | `ToXYIndex(radius, origin, Layout.OddR)` |
| `ToEvenRXYIndex(radius, origin)` | `ToXYIndex(radius, origin, Layout.EvenR)` |
| `ToOddQXYIndex(radius, origin)` | `ToXYIndex(radius, origin, Layout.OddQ)` |
| `ToEvenQXYIndex(radius, origin)` | `ToXYIndex(radius, origin, Layout.EvenQ)` |

Do not select a helper merely from the visual orientation: `OddR` and `EvenR`, or `OddQ` and
`EvenQ`, can label the same physical cell with different offset indices. The helper must match
the topology's complete layout.

## Validation and Boundaries

`ToQRSIndex(Layout)` throws `ArgumentOutOfRangeException` when `Q` or `R` is not finite, when the
derived `S` is not finite, when a rounded or corrected component does not fit in `Int32`, or when
the layout value is undefined.

The explicit cast does not provide this nearest-cell validation contract. Use it only with
finite `Q` and `R` values that are safely convertible to `Int32` and whose derived `S` also fits
in `Int32`.

The general point-classification method and all four layout-specific helpers require:

- finite `PointXY` coordinates;
- a finite `hexRadius` greater than zero; and
- a finite `VectorXY` origin.

The general method also rejects an undefined layout. These failures are reported as
`ArgumentOutOfRangeException`.

Keep these boundary behaviors in mind:

- The grid is conceptually unbounded. `ToXYIndex` can return negative indices or indices outside
  a particular map; validate the result against that map's resolution before indexing storage.
- A point exactly on an edge or vertex is assigned to one cell by the deterministic tie rule. A
  very small floating-point change near that boundary can select the neighboring cell.
- Finite inputs can still produce an impractically large normalized coordinate when the point is
  extremely far from the origin or the radius is extremely small. Keep the projected index in
  the `Int32` range and at a scale where `float` precision is sufficient.
- Mixing the radius, origin, or layout from another grid silently changes the selected index; the
  method cannot detect that semantic mismatch.

Continue with [Rotations and Transformations](rotations-and-transformations.md) for operations
that intentionally keep coordinates fractional.
