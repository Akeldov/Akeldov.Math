# QRS Coordinates

QRS coordinates describe the hex lattice with three axes separated by 120 degrees. They are
cube coordinates restricted to the plane

```text
Q + R + S = 0
```

Only two components are independent. Akeldov.Math.Hexes uses `Q` and `R` as constructor
arguments and derives `S`, while exposing all three components for symmetric calculations.

## Choose integer or fractional QRS

Akeldov.Math.Hexes provides two immutable value types in the
`Akeldov.Math.Hexes.Vectors.QRS` namespace:

| Type | Components | Use |
|---|---|---|
| <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> | `int` | Cell coordinates, exact grid offsets, dictionary keys, and 60-degree rotations |
| <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> | `float` | Values between cell centers, interpolation, scaling, arbitrary rotation, and rounding |

Create either value from its independent components:

```csharp
using Akeldov.Math.Hexes.Vectors.QRS;

var cell = new VectorQRSInt(q: 2, r: -1);
var sample = new VectorQRS(q: 1.5f, r: -0.25f);

int cellS = cell.S;       // -1
float sampleS = sample.S; // -1.25
```

`VectorQRSInt` also has a three-component constructor. It accepts the value only when the sum is
zero:

```csharp
var sameCell = new VectorQRSInt(q: 2, r: -1, s: -1);
```

The two-component integer constructor rejects values whose derived `S` does not fit in `Int32`.
The three-component constructor rejects any nonzero sum.

## Work with immutable values

Both QRS types are `readonly struct` values. Assignment copies the value, their properties cannot
be changed, and arithmetic returns a new coordinate:

```csharp
var start = new VectorQRSInt(1, -2);
var offset = new VectorQRSInt(2, 0);

VectorQRSInt destination = start + offset; // Q/R/S: 3, -2, -1
VectorQRSInt difference = destination - start; // Q/R/S: 2, 0, -2
VectorQRSInt doubled = offset * 2; // Q/R/S: 4, 0, -4
```

Integer addition, subtraction, and multiplication check `Q` and `R` for overflow and throw
`OverflowException` instead of wrapping them to a different cell. If those components fit but the
derived `S` does not, construction throws `ArgumentOutOfRangeException`. Integer division divides
`Q` and `R` with normal truncating integer division and derives a new `S`; convert to `VectorQRS`
first when fractional scaling is required.

Fractional arithmetic uses ordinary single-precision floating-point rules:

```csharp
var value = new VectorQRS(3f, -1f);

VectorQRS half = value / 2f; // (1.5, -0.5, -1)
VectorQRS moved = half + new VectorQRS(0.25f, 0.5f);
```

The fractional constructor does not reject `NaN` or infinity. Operations that require a finite
coordinate, such as discretization and geometric conversion, validate their arguments when the
public method is called.

## Equality and deconstruction

`Equals`, `==`, and `!=` compare the independent components exactly. The derived `S` is fixed by
the invariant, so comparing `Q` and `R` is sufficient. Exact equality makes `VectorQRSInt` a
natural key for dictionaries and sets.

Deconstruction returns the two independent components:

```csharp
var coordinate = new VectorQRSInt(2, -1);
var (q, r) = coordinate;
int s = coordinate.S;
```

Use an explicit tolerance in application code when comparing fractional values produced by
different floating-point calculations.

## Convert between integer and fractional values

Conversion from `VectorQRSInt` to `VectorQRS` is implicit because it does not intentionally
discard a fractional part:

```csharp
VectorQRSInt cell = new VectorQRSInt(2, -1);
VectorQRS fractional = cell;
```

`VectorQRS` uses `float` components, so very large integer coordinates can lose precision during
this conversion.

The reverse conversion is explicit and truncates `Q` and `R` toward zero:

```csharp
var fractional = new VectorQRS(2.8f, -1.2f);
var truncated = (VectorQRSInt)fractional; // Q/R/S: 2, -1, -1
```

Truncation is not nearest-hex rounding. Use `ToQRSIndex(Layout)` when selecting the closest cell;
see [Coordinate Discretization](../coordinate-discretization.md).

## Measure grid distance

The distance between two hexes is the smallest number of edge-adjacent steps between them. For a
QRS difference it is the largest absolute component:

```csharp
using System;

var from = new VectorQRSInt(0, 0);
var to = new VectorQRSInt(2, -1);

long deltaQ = (long)to.Q - from.Q;
long deltaR = (long)to.R - from.R;
long deltaS = (long)to.S - from.S;

long distance = Math.Max(
    Math.Abs(deltaQ),
    Math.Max(Math.Abs(deltaR), Math.Abs(deltaS)));

// distance == 2
```

The equivalent expression is `(abs(deltaQ) + abs(deltaR) + abs(deltaS)) / 2`. Calculate the
differences in `long`, as above, so that subtraction and `Math.Abs` remain valid for every
`VectorQRSInt` component. Version 0.1.0 does not provide a dedicated QRS distance method, so keep
this calculation in application code when it is needed.

The six unit offsets to edge-adjacent hexes are `(1, 0, -1)`, `(1, -1, 0)`,
`(0, -1, 1)`, `(-1, 0, 1)`, `(-1, 1, 0)`, and `(0, 1, -1)`. Layout-aware adjacency helpers are
available for row-and-column indices; see [Row and Column Indices](row-and-column-indices.md).

## Relate QRS to layout

A QRS value does not store row or column staggering. Odd and even variants of the same
orientation therefore use the same QRS axes. A <xref:Akeldov.Math.Hexes.Layout> is still required
when QRS is converted to a rectangular index or to an oriented spatial basis.

See [Layouts](../layouts.md) for orientation and parity, [Spatial Coordinates](spatial-coordinates.md)
for QRS/XY geometry conversion, and [Rotations and Transformations](../rotations-and-transformations.md)
for exact and arbitrary-angle rotations.
