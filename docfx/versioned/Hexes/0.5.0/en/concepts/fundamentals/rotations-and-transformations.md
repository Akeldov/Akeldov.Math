# Rotations and Transformations

Akeldov.Math.Hexes provides two related kinds of rotation:

- exact 60-degree rotations in QRS space, which preserve the hex lattice;
- floating-point rotations and affine transformations for continuous coordinates.

All positive angles rotate counterclockwise. QRS rotations are independent of
[layout](layouts.md): no `Layout` argument is required.

## Sixfold angles

<xref:Akeldov.Math.Hexes.SixfoldAngle> represents the six rotations that preserve a regular
hex grid.

| Value | Numeric value | Degrees | Radians |
|---|---:|---:|---:|
| `Deg0` | 0 | 0 | 0 |
| `Deg60` | 1 | 60 | π / 3 |
| `Deg120` | 2 | 120 | 2π / 3 |
| `Deg180` | 3 | 180 | π |
| `Deg240` | 4 | 240 | 4π / 3 |
| `Deg300` | 5 | 300 | 5π / 3 |

The angle helpers expose the corresponding `float` values and modular operations:

- `Sin()` and `Cos()` return the stored trigonometric values;
- `AsFloatRadians()` returns a value in `[0, 2π)`;
- `AsFloatDegrees()` returns a value in `[0, 360)`;
- `Negate()` returns the inverse rotation, so `Deg60` becomes `Deg300`;
- `Add60()`, `Add120()`, `Add180()`, `Add240()`, and `Add300()` add the named angle and
  wrap at 360 degrees.

`Negate()` is not the same operation as `Add180()`. The former undoes a rotation; the latter
turns it by another half-circle.

```csharp
using Akeldov.Math.Hexes;

SixfoldAngle angle = SixfoldAngle.Deg300.Add120(); // Deg60
SixfoldAngle inverse = angle.Negate();              // Deg300
float radians = angle.AsFloatRadians();             // Approximately 1.0471976
float sine = angle.Sin();                            // Approximately 0.8660254

SixfoldAngle[] ordered = SixfoldAngles.All;
```

<xref:Akeldov.Math.Hexes.SixfoldAngles.All> returns the values in counterclockwise order from
`Deg0` through `Deg300`. Each access returns a new mutable array owned by the caller. Changing
that array does not change a later result.

An enum cast can still create an undefined value such as `(SixfoldAngle)42`. Angle lookup,
addition, and rotation helpers reject undefined values with `ArgumentOutOfRangeException`.

## Counterclockwise convention

The XY helpers use the standard rotation matrix:

```text
x' = x cos(angle) - y sin(angle)
y' = x sin(angle) + y cos(angle)
```

Consequently, positive angles are counterclockwise in a coordinate system whose positive Y
axis points upward. If the result is displayed in screen or raster coordinates whose Y axis
points downward, the same numeric transformation appears clockwise.

The overload type determines the angle unit. A `SixfoldAngle` is one of the named degree
steps, while a `float angleRad` is in radians. There is no floating-point degree overload:
`Rotate(60f)` means 60 radians, not 60 degrees.

## Exact QRS rotations

Both <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> and
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> can be rotated about the QRS origin with a
`SixfoldAngle`. These overloads permute and negate components instead of evaluating sine and
cosine.

For `v = (Q, R, S)`, the results are:

| Angle | Result `(Q, R, S)` |
|---|---|
| `Deg0` | `(Q, R, S)` |
| `Deg60` | `(-R, -S, -Q)` |
| `Deg120` | `(S, Q, R)` |
| `Deg180` | `(-Q, -R, -S)` |
| `Deg240` | `(R, S, Q)` |
| `Deg300` | `(-S, -Q, -R)` |

The zero-sum invariant `Q + R + S = 0` is therefore preserved. The return type also preserves the
input kind: rotating a `VectorQRS` returns `VectorQRS`, and rotating a `VectorQRSInt` returns
`VectorQRSInt` when every rotated component fits in `Int32`. An out-of-range integer result is
rejected with `ArgumentOutOfRangeException`.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;

var index = new VectorQRSInt(2, -5); // S is 3

VectorQRSInt rotated = index.Rotate(SixfoldAngle.Deg60);
// rotated is (5, -3), with S equal to -2
```

QRS rotation overloads always rotate about the origin. To rotate about another QRS index,
translate to the pivot, rotate, and translate back:

```csharp
var pivot = new VectorQRSInt(1, -1);
var point = new VectorQRSInt(3, -2);

VectorQRSInt aroundPivot =
    (point - pivot).Rotate(SixfoldAngle.Deg120) + pivot;
```

## Arbitrary QRS rotations

The `Rotate(float angleRad)` overload accepts any finite angle in radians. It performs the
rotation in the QRS coordinate plane and returns fractional QRS coordinates:

| Receiver | Call | Return type |
|---|---|---|
| `VectorQRS` | `Rotate(float angleRad)` | `VectorQRS` |
| `VectorQRSInt` | `Rotate(float angleRad)` | `VectorQRS` |

```csharp
using System;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;

var index = new VectorQRSInt(2, -5);

VectorQRSInt exactStep = index.Rotate(SixfoldAngle.Deg60);
VectorQRS arbitrary = index.Rotate(MathF.PI / 6f); // 30 degrees
```

The integer overload deliberately returns `VectorQRS`: an arbitrary rotation does not usually
land on an integer hex index. If a discrete index is required afterward, round explicitly with
`ToQRSIndex(Layout)` as described in
[Coordinate Discretization](coordinate-discretization.md). An explicit cast to `VectorQRSInt`
only truncates `Q` and `R` toward zero and is not nearest-hex rounding.

The arbitrary-angle overload rejects a non-finite angle. For a `VectorQRS` receiver it also
rejects non-finite QRS components.

## XY rotations

The Hexes XY extensions rotate only by `SixfoldAngle`; they do not provide an arbitrary-radian
XY overload. The receiver and pivot can be floating-point or integer values, but every overload
returns `VectorXY`.

| Receiver | Center of rotation | Pivot type | Return type |
|---|---|---|---|
| `VectorXY` | Origin | — | `VectorXY` |
| `VectorXYInt` | Origin | — | `VectorXY` |
| `VectorXY` | Explicit pivot | `VectorXY` or `VectorXYInt` | `VectorXY` |
| `VectorXYInt` | Explicit pivot | `VectorXY` or `VectorXYInt` | `VectorXY` |

The pivot overload places the pivot before the angle: `point.Rotate(pivot, angle)`. The pivot
remains fixed, and only the point-to-pivot offset is rotated.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

var point = new VectorXY(3f, 1f);
var pivot = new VectorXY(1f, 1f);

VectorXY result = point.Rotate(pivot, SixfoldAngle.Deg60);
// result is approximately (2, 2.7320508)

VectorXY fromInteger =
    new VectorXYInt(2, -3).Rotate(SixfoldAngle.Deg180);
// fromInteger is (-2, 3), but its type is VectorXY
```

Returning `VectorXY` is necessary because 60-degree XY rotations generally have fractional
components. Even `VectorXYInt.Rotate(Deg180)` has the compile-time return type `VectorXY`.

## Affine `Transform`

`Transform` is available on both `VectorXY` and `VectorXYInt`. Every overload returns
`VectorXY` and accepts its translation as either `VectorXY` or `VectorXYInt`.

There are two operation sequences:

```text
Transform(angle, offset)
    result = Rotate(point, angle) + offset

Transform(scaleFactor, angle, offset)
    result = Rotate(point * scaleFactor, angle) + offset
```

The scale is uniform and is applied first. Rotation is about the origin. Translation is applied
last, so the offset itself is neither scaled nor rotated.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

var source = new VectorXY(2f, 0f);
var translation = new VectorXY(10f, 20f);

VectorXY transformed = source.Transform(
    scaleFactor: 2f,
    angle: SixfoldAngle.Deg60,
    offset: translation);

// Scale:       (2, 0) -> (4, 0)
// Rotate 60°:  (4, 0) -> approximately (2, 3.464102)
// Translate:               approximately (12, 23.464102)
```

`Transform` has no pivot parameter and no arbitrary-radian overload. For a pivoted operation,
translate the value relative to the pivot, apply the required scale or rotation, and translate
it back explicitly.

## Practical cautions

- Use a `SixfoldAngle` overload for named degree steps; numeric QRS angles are radians.
- Sixfold QRS rotations preserve integer indices when the rotated components fit in `Int32`;
  out-of-range integer results are rejected. Arbitrary QRS rotations return fractional coordinates.
- All XY rotation and transformation overloads return `VectorXY`, including those called on
  `VectorXYInt`.
- Floating-point XY results use trigonometric constants and should normally be compared with a
  tolerance.
- `Transform` always applies translation last and always rotates about the origin.
- Rotation helpers return new values; none of the immutable vector values is modified in place.

See [QRS Coordinates](coordinate-systems/qrs-coordinates.md) for the coordinate invariant and
[Layouts](layouts.md) for the grid orientations used when QRS values are later mapped to space.
