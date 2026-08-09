# VectorXY

`VectorXY` represents a two-dimensional vector with single-precision floating-point components. Use it for geometric directions, offsets, sizes, and calculations that can produce fractional values.

In Spatial2D's Cartesian coordinate model, a `PointXY` identifies a position while a `VectorXY` identifies an offset or direction. Vector components and lengths use the same world units as the points and geometry with which they operate. Raster grids use `VectorXY` values for world-space sizes and bounds.

## Construction and Components

```csharp
using Akeldov.Math.Spatial2D;

var vector = new VectorXY(3f, 4f);

float x = vector.X;                       // 3
float y = vector.Y;                       // 4
float length = vector.Length;             // 5
float squaredLength = vector.SquaredLength; // 25
bool isFinite = vector.IsFinite;           // true

VectorXY zero = VectorXY.Zero;     // (0, 0)
VectorXY one = VectorXY.One;       // (1, 1)
VectorXY basisX = VectorXY.BasisX; // (1, 0)
VectorXY basisY = VectorXY.BasisY; // (0, 1)
```

`IsFinite` is `false` when either component is NaN or positive or negative infinity. `BasisX` and `BasisY` are the standard Cartesian basis vectors along the positive X and Y axes.

The vector can be deconstructed into its components:

```csharp
var (x, y) = vector; // x = 3, y = 4
```

## Arithmetic

```csharp
var a = new VectorXY(3f, 4f);
var b = new VectorXY(1f, 2f);

VectorXY sum = a + b;          // (4, 6)
VectorXY difference = a - b;   // (2, 2)

VectorXY scaledA = a * 2f;     // (6, 8)
VectorXY scaledB = 2f * a;     // (6, 8)
VectorXY integerScaledA = a * 2; // (6, 8)
VectorXY integerScaledB = 2 * a; // (6, 8)
VectorXY divided = a / 2f;     // (1.5, 2)
```

All arithmetic operators return `VectorXY`. Division follows floating-point rules; division by zero produces infinity or NaN components rather than throwing `DivideByZeroException`.

## Normalization

`Normalize` preserves direction and returns a vector of length one:

```csharp
VectorXY direction = new VectorXY(3f, 4f).Normalize(); // (0.6, 0.8)
```

Normalizing `VectorXY.Zero` returns `VectorXY.Zero`.

## Equality and Formatting

`VectorXY` implements `IEquatable<VectorXY>`. `Equals`, `==`, and `!=` compare components exactly:

```csharp
var a = new VectorXY(1f, 2f);
var b = new VectorXY(1f, 2f);

bool equal = a == b;          // true
bool different = a != b;     // false
bool alsoEqual = a.Equals(b); // true
```

Use `AlmostEquals` for tolerance-based comparison. It compares the Euclidean distance using `GeometryConstants.GeometryEpsilon` by default:

```csharp
bool almostEqual = new VectorXY(1f, 2f).AlmostEquals(
    new VectorXY(1.000001f, 2f)); // true with the default tolerance

bool customTolerance = new VectorXY(1f, 2f).AlmostEquals(
    new VectorXY(1.01f, 2f),
    epsilon: 0.02f); // true
```

`GetHashCode` is based on both components. `ToString` uses invariant culture and produces the form `(X, Y)`.

## Dot Product, Cross Product, and Angle

```csharp
VectorXY right = VectorXY.BasisX;
VectorXY up = VectorXY.BasisY;

float dot = VectorXY.Dot(right, up);       // 0
float cross = VectorXY.Cross(right, up);   // 1
float angleRad = VectorXY.Angle(right, up); // π / 2
```

`Cross` returns the signed scalar two-dimensional cross product. `Angle(from, to)` returns the signed angle in radians: counterclockwise rotation is positive and clockwise rotation is negative.

## Conversion to and from VectorXYInt

Conversion from `VectorXYInt` to `VectorXY` is implicit:

```csharp
var integer = new VectorXYInt(3, 2);
VectorXY floating = integer; // (3, 2)
```

Conversion from `VectorXY` to `VectorXYInt` is explicit because it discards fractional components:

```csharp
var floating = new VectorXY(3.8f, -2.4f);
var truncated = (VectorXYInt)floating; // (3, -2)
```

Use `RoundToInt` when components should be rounded instead. It uses `MathF.Round` midpoint-to-even semantics:

```csharp
VectorXYInt rounded = new VectorXY(2.5f, 3.5f).RoundToInt(); // (2, 4)
```

## Distance

`Distance` calculates Euclidean distance to either a floating-point or integer vector:

```csharp
var origin = new VectorXY(1f, 1f);

float floatingDistance = origin.Distance(new VectorXY(4f, 5f));    // 5
float integerDistance = origin.Distance(new VectorXYInt(4, 5));    // 5
```

## Rotation

`Rotate` rotates the vector around the origin. The angle is expressed in radians:

```csharp
using System;

VectorXY rotated = VectorXY.BasisX.Rotate(MathF.PI / 2f); // approximately (0, 1)
```

The method throws `ArgumentOutOfRangeException` when the angle is NaN or infinite.

## Hadamard Operations

Hadamard operations multiply or divide corresponding components:

```csharp
var floatingA = new VectorXY(6f, 8f);
var floatingB = new VectorXY(2f, 4f);
var integer = new VectorXYInt(3, 2);

VectorXY floatingProduct = floatingA.HadamardMultiply(floatingB); // (12, 32)
VectorXY integerProduct = floatingA.HadamardMultiply(integer);    // (18, 16)

VectorXY floatingQuotient = floatingA.HadamardDivide(floatingB); // (3, 2)
VectorXY integerQuotient = floatingA.HadamardDivide(integer);    // (2, 4)
```

## Clamping

`Clamp` restricts each component to an inclusive floating-point range:

```csharp
var source = new VectorXY(12.5f, -3.5f);

VectorXY clamped = source.Clamp(
    new VectorXY(0f, 0f),
    new VectorXY(10f, 10f)); // (10, 0)
```

The minimum and maximum can also be applied independently. These overloads accept either `VectorXY` or `VectorXYInt`:

```csharp
VectorXY atLeast = source.ClampMin(new VectorXY(0f, 0f));    // (12.5, 0)
VectorXY atMost = source.ClampMax(new VectorXY(10f, 10f));   // (10, -3.5)

VectorXY integerMin = source.ClampMin(new VectorXYInt(0, 0));   // (12.5, 0)
VectorXY integerMax = source.ClampMax(new VectorXYInt(10, 10)); // (10, -3.5)
```

`Clamp` throws `ArgumentException` when a maximum component is smaller than its corresponding minimum component.

## Rounding

`Round` rounds both components to the specified number of fractional digits:

```csharp
VectorXY rounded = new VectorXY(1.234f, 5.678f).Round(2); // (1.23, 5.68)
```

The array overload returns a new caller-owned array and does not modify the source array:

```csharp
var source = new[]
{
    new VectorXY(1.24f, 2.76f),
    new VectorXY(3.55f, 4.44f)
};

VectorXY[] rounded = source.Round(1); // [(1.2, 2.8), (3.6, 4.4)]
```

## Sequence Aggregation

`Sum` and `Average` aggregate `IEnumerable<VectorXY>` sequences:

```csharp
var vectors = new[]
{
    new VectorXY(1f, 2f),
    new VectorXY(3f, 4f)
};

VectorXY sum = vectors.Sum();         // (4, 6)
VectorXY average = vectors.Average(); // (2, 3)
```

`Sum` returns `VectorXY.Zero` for an empty sequence. `Average` throws `InvalidOperationException` for an empty sequence.

## Binary Serialization

The binary extensions write `X` and `Y` as two consecutive `Single` values:

```csharp
using System.IO;

using var stream = new MemoryStream();
using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
{
    writer.Write(new VectorXY(3.5f, 4.25f));
}

stream.Position = 0;
using var reader = new BinaryReader(stream);
VectorXY restored = reader.ReadVectorXY(); // (3.5, 4.25)
```

Passing a null reader or writer to these extensions throws `ArgumentNullException`.
