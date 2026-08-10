# Vectors

Akeldov.Math.Spatial2D provides two Cartesian vector types:

- <xref:Akeldov.Math.Spatial2D.VectorXY> stores single-precision floating-point components for
  directions, offsets, sizes, and other continuous quantities.
- <xref:Akeldov.Math.Spatial2D.VectorXYInt> is the integer-component counterpart of `VectorXY`.
  It represents integer directions, offsets, sizes, and API-specific values such as raster
  indices and resolutions.

Both are immutable value types. Their `X` and `Y` components follow the same Cartesian axes as
[points](points.md), but a vector describes a direction or displacement rather than a position.

## Choose a vector type

Use `VectorXY` for geometric calculations and whenever an operation can produce a fractional
result. Use `VectorXYInt` when integer components are part of the domain contract rather than an
incidental representation.

| Property | `VectorXY` | `VectorXYInt` |
|---|---|---|
| Component type | `float` | `int` |
| Typical meaning | Direction, displacement, world-space size | Integer direction, displacement, size, index, or resolution |
| Scalar division | Floating-point division | Integer division |
| Rotation result | `VectorXY` | `VectorXY` |
| Conversion from the other type | Implicit; very large integers may lose precision | Explicit and potentially lossy |

Do not use `VectorXYInt` merely because the current values happen to be whole numbers. For
example, a direction can become fractional after normalization or rotation, so it should remain
a `VectorXY`. See [Discrete Indices](discrete-indices.md) for values that identify cells or
elements of a discrete structure.

## Create and inspect vectors

Pass Cartesian components to the constructors:

```csharp
using Akeldov.Math.Spatial2D;

var offset = new VectorXY(3f, 4f);
var resolution = new VectorXYInt(192, 128);

float offsetX = offset.X; // 3
float offsetY = offset.Y; // 4

int width = resolution.X;  // 192
int height = resolution.Y; // 128
```

Both types are `readonly struct` values. Assigning a vector or passing it to a method copies its
components. An arithmetic or transformation operation returns a new vector instead of changing
the original.

The standard constants describe the origin, equal components, and Cartesian basis directions:

```csharp
VectorXY zero = VectorXY.Zero;     // (0, 0)
VectorXY one = VectorXY.One;       // (1, 1)
VectorXY right = VectorXY.BasisX;  // (1, 0)
VectorXY up = VectorXY.BasisY;     // (0, 1)

VectorXYInt gridRight = VectorXYInt.BasisX; // (1, 0)
VectorXYInt gridUp = VectorXYInt.BasisY;    // (0, 1)
```

Vectors can also be deconstructed:

```csharp
var (x, y) = new VectorXY(3f, 4f);
```

`VectorXY` accepts all `float` values. Use `IsFinite` at a boundary where `NaN` or infinity is
not meaningful:

```csharp
bool usableForGeometry = offset.IsFinite; // true
```

## Distinguish positions from displacements

A <xref:Akeldov.Math.Spatial2D.PointXY> identifies a location. A vector can translate that
location, and subtracting two points produces the displacement between them:

```csharp
var start = new PointXY(2f, 3f);
var displacement = new VectorXY(4f, -1f);

PointXY end = start + displacement;       // (6, 2)
VectorXY measured = end - start;          // (4, -1)
PointXY restored = end - displacement;    // (2, 3)
```

Vectors can be added because displacements compose. Points cannot be added because the sum of
two positions has no unambiguous geometric meaning.

## Add, subtract, and scale

Vector arithmetic operates component by component:

```csharp
var a = new VectorXY(3f, 4f);
var b = new VectorXY(1f, 2f);

VectorXY sum = a + b;        // (4, 6)
VectorXY difference = a - b; // (2, 2)
VectorXY doubled = a * 2f;   // (6, 8)
VectorXY halved = a / 2f;    // (1.5, 2)
```

Floating-point division follows the normal .NET rules, so division by zero can produce `NaN`
or infinity. Check `IsFinite` if such a result cannot be accepted by the next operation.

Integer vector arithmetic remains integral when the scalar is an integer:

```csharp
var size = new VectorXYInt(7, 5);

VectorXYInt doubled = size * 2; // (14, 10)
VectorXYInt halved = size / 2;  // (3, 2)
VectorXY scaled = size * 0.5f;  // (3.5, 2.5)
```

Integer division truncates each quotient toward zero and division by zero throws
`DivideByZeroException`. Multiplication by a floating-point scalar returns `VectorXY` so that a
fractional result is not discarded.

## Work with length and direction

`Length` is the Euclidean magnitude of either vector type. `VectorXY.SquaredLength` avoids a
square root when magnitudes only need to be compared:

```csharp
var vector = new VectorXY(3f, 4f);

float length = vector.Length;               // 5
float squaredLength = vector.SquaredLength; // 25
```

`Normalize` preserves the direction and returns a vector of length one:

```csharp
VectorXY direction = new VectorXY(3f, 4f).Normalize(); // (0.6, 0.8)
```

The zero vector has no unique geometric direction. Spatial2D handles this boundary case by
returning `VectorXY.Zero` when it is normalized.

Use `Distance` when vectors are being treated as coordinate values and you need the Euclidean
distance between them:

```csharp
float distance = new VectorXY(1f, 1f).Distance(
    new VectorXYInt(4, 5)); // 5
```

## Relate directions

The dot product measures alignment. Perpendicular vectors have a dot product of zero, while the
sign distinguishes broadly matching and opposing directions.

The scalar two-dimensional cross product measures signed orientation. A positive result means
the right-hand vector is counterclockwise from the left-hand vector; a negative result means it
is clockwise.

```csharp
VectorXY right = VectorXY.BasisX;
VectorXY up = VectorXY.BasisY;

float dot = VectorXY.Dot(right, up);         // 0
float cross = VectorXY.Cross(right, up);     // 1
float angle = VectorXY.Angle(right, up);     // PI / 2
```

`Angle(from, to)` combines these values with `Atan2` and returns the signed shortest angle in
radians. Counterclockwise angles are positive and clockwise angles are negative.

## Rotate vectors

`Rotate` rotates a vector around the origin. Angles are expressed in radians:

```csharp
VectorXY rotated = VectorXY.BasisX.Rotate(MathF.PI / 2f);
// Approximately (0, 1)
```

Rotation generally produces fractional components, so rotating `VectorXYInt` also returns
`VectorXY`:

```csharp
VectorXY diagonal = new VectorXYInt(1, 0).Rotate(MathF.PI / 4f);
// Approximately (0.7071, 0.7071)
```

The angle must be finite. `NaN` and positive or negative infinity cause
`ArgumentOutOfRangeException`.

## Convert at the discrete boundary

Conversion from `VectorXYInt` to `VectorXY` is implicit because it preserves the vector's
continuous meaning and does not discard a fractional part:

```csharp
var integer = new VectorXYInt(3, 2);
VectorXY continuous = integer; // (3, 2)
```

Single-precision floating-point values cannot represent every large `int` exactly. Components
whose magnitude exceeds the exact integer range of `float` can therefore be rounded during this
otherwise widening conversion.

Conversion in the other direction is explicit because it discards fractional components by
truncating them toward zero:

```csharp
var continuous = new VectorXY(3.8f, -2.4f);
var truncated = (VectorXYInt)continuous; // (3, -2)
```

Use `RoundToInt` when the nearest integer components are intended instead. It uses
`MathF.Round` and therefore rounds midpoint values to the nearest even integer:

```csharp
VectorXYInt rounded = new VectorXY(2.5f, 3.5f).RoundToInt();
// (2, 4)
```

Keep calculations in `VectorXY` until the operation that actually requires integral values.
This makes the lossy conversion visible and avoids accumulating truncation error between steps.

## Use exact or tolerant equality

Both vector types have structural value equality. `Equals`, `==`, and `!=` compare their
components exactly, and their hash codes follow the same rule:

```csharp
bool sameFloatVector = new VectorXY(1f, 2f) == new VectorXY(1f, 2f); // true
bool sameIntVector = new VectorXYInt(1, 2) == new VectorXYInt(1, 2); // true
```

Use `AlmostEquals` for `VectorXY` values produced by floating-point calculations. The tolerance
is an inclusive Euclidean distance and defaults to `GeometryConstants.GeometryEpsilon`:

```csharp
bool almostEqual = new VectorXY(1f, 2f).AlmostEquals(
    new VectorXY(1.000001f, 2f));

bool withinCustomTolerance = new VectorXY(1f, 2f).AlmostEquals(
    new VectorXY(1.01f, 2f),
    epsilon: 0.02f); // true
```

Use exact equality for dictionary keys and discrete vectors. Use tolerant comparison explicitly
when geometric proximity is the intended relationship.

For complete member lists, see the API references for
<xref:Akeldov.Math.Spatial2D.VectorXY>,
<xref:Akeldov.Math.Spatial2D.VectorXYInt>,
<xref:Akeldov.Math.Spatial2D.VectorXYExtensions>, and
<xref:Akeldov.Math.Spatial2D.VectorXYIntExtensions>.
