# VectorXYInt

`VectorXYInt` represents a two-dimensional vector with integer components. Use it for raster resolutions, grid dimensions, discrete offsets, and other values that must remain integral.

## Construction and Components

```csharp
using Akeldov.Math.Spatial2D;

var vector = new VectorXYInt(6, 4);

int x = vector.X;             // 6
int y = vector.Y;             // 4
float length = vector.Length; // ≈ 7.211

VectorXYInt zero = VectorXYInt.Zero; // (0, 0)
VectorXYInt one = VectorXYInt.One;   // (1, 1)
VectorXYInt basisX = VectorXYInt.BasisX; // (1, 0)
VectorXYInt basisY = VectorXYInt.BasisY; // (0, 1)
```

`Length` returns the Euclidean length as a `float`.

`BasisX` and `BasisY` are the standard Cartesian basis vectors along the positive X and Y axes.

The vector can be deconstructed into its components:

```csharp
var (x, y) = vector; // x = 6, y = 4
```

## Arithmetic

```csharp
var a = new VectorXYInt(6, 4);
var b = new VectorXYInt(2, 1);

VectorXYInt sum = a + b;               // (8, 5)
VectorXYInt difference = a - b;        // (4, 3)

VectorXYInt scaledA = a * 2;           // (12, 8)
VectorXYInt scaledB = 2 * a;           // (12, 8)
VectorXYInt divided = a / 2;           // (3, 2)
VectorXYInt truncatedDivided = a / 3;   // (2, 1), not (2, 1.333...)
```

Operations with an integer scalar return `VectorXYInt`. Division follows C# integer division rules, so `(7, 5) / 2` produces `(3, 2)`. Division by zero throws `DivideByZeroException`.

Scaling by a floating-point value preserves the fractional result and returns `VectorXY`:

```csharp
VectorXY scaledA = a * 1.5f; // (9, 6)
VectorXY scaledB = 1.5f * a; // (9, 6)
```

## Equality and Formatting

`VectorXYInt` implements `IEquatable<VectorXYInt>` and supports value equality:

```csharp
bool equal = a == b;          // false
bool different = a != b;     // true
bool alsoEqual = a.Equals(b); // false
```

`GetHashCode` is based on both components. `ToString` uses invariant culture and produces the form `(X, Y)`.

## Conversion to and from VectorXY

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

Use `RoundToInt` when components should be rounded instead:

```csharp
VectorXYInt rounded = floating.RoundToInt(); // (4, -2)
```

## Distance

`Distance` calculates Euclidean distance to either an integer or floating-point vector:

```csharp
float integerDistance = a.Distance(new VectorXYInt(9, 8));       // 5
float floatingDistance = a.Distance(new VectorXY(9.5f, 8.25f)); // ≈ 5.506
```

## Rotation

`Rotate` rotates the vector around the origin. The angle is expressed in radians and the result is a `VectorXY` because rotation can produce fractional components:

```csharp
using System;

VectorXY rotated = new VectorXYInt(1, 0).Rotate(MathF.PI / 2f); // approximately (0, 1)
```

The method throws `ArgumentOutOfRangeException` when the angle is NaN or infinite.

## Hadamard Operations

Hadamard operations multiply or divide corresponding components:

```csharp
var integerA = new VectorXYInt(6, 8);
var integerB = new VectorXYInt(2, 4);
var floating = new VectorXY(1.5f, 2.5f);

VectorXYInt integerProduct = integerA.HadamardMultiply(integerB); // (12, 32)
VectorXY floatingProduct = integerA.HadamardMultiply(floating);   // (9, 20)

VectorXY integerQuotient = integerA.HadamardDivide(integerB);   // (3, 2)
VectorXY floatingQuotient = integerA.HadamardDivide(floating);  // (4, 3.2)
```

Division returns `VectorXY` even when both operands are integer vectors, preserving fractional quotients.

## Clamping

`Clamp` restricts each component to an inclusive range:

```csharp
var source = new VectorXYInt(12, -3);

VectorXYInt clamped = source.Clamp(
    new VectorXYInt(0, 0),
    new VectorXYInt(10, 10)); // (10, 0)
```

The minimum and maximum can also be applied independently:

```csharp
VectorXYInt atLeast = source.ClampMin(new VectorXYInt(0, 0));   // (12, 0)
VectorXYInt atMost = source.ClampMax(new VectorXYInt(10, 10)); // (10, -3)

VectorXYInt atLeastComponents = source.ClampMin(0, 0);   // (12, 0)
VectorXYInt atMostComponents = source.ClampMax(10, 10); // (10, -3)
```

`Clamp` throws `ArgumentException` when a maximum component is smaller than its corresponding minimum component.

## Binary Serialization

The binary extensions write `X` and `Y` as two consecutive `Int32` values:

```csharp
using System.IO;

using var stream = new MemoryStream();
using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
{
    writer.Write(new VectorXYInt(6, 4));
}

stream.Position = 0;
using var reader = new BinaryReader(stream);
VectorXYInt restored = reader.ReadVectorXYInt(); // (6, 4)
```

Passing a null reader or writer to these extensions throws `ArgumentNullException`.
