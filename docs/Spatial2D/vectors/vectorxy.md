# VectorXY

`VectorXY` represents a two-dimensional vector with single-precision floating-point components. Use it for geometric directions, offsets, sizes, and calculations that can produce fractional values.

## Basic Operations

```csharp
using Akeldov.Math.Spatial2D;

var a = new VectorXY(3f, 4f);
var b = new VectorXY(1f, 2f);

VectorXY sum = a + b;
VectorXY difference = a - b;
VectorXY scaled = a * 2f;

float length = a.Length;
float squaredLength = a.SquaredLength;
VectorXY direction = a.Normalize();
```

`VectorXY.Zero` and `VectorXY.One` provide the commonly used constant vectors.

## Dot, Cross, and Angle

```csharp
var right = new VectorXY(1f, 0f);
var up = new VectorXY(0f, 1f);

float dot = VectorXY.Dot(right, up);
float cross = VectorXY.Cross(right, up);
float angleRad = VectorXY.Angle(right, up);
```

`VectorXY.Angle(from, to)` returns a signed angle in radians.

## Extension Methods

The vector extensions cover common geometry tasks:

- `Distance` for Euclidean distance.
- `Rotate` for rotation around zero or a pivot.
- `Transform` for scale, rotation, and offset.
- `Clamp`, `ClampMin`, and `ClampMax`.
- `HadamardMultiply` and `HadamardDivide`.
- `Round` for component rounding.
- `Sum` and `Average` for vector sequences.

```csharp
using System;

var vector = new VectorXY(10f, 0f);
VectorXY rotated = vector.Rotate(MathF.PI / 2f);
VectorXY clamped = rotated.Clamp(
    new VectorXY(0f, 0f),
    new VectorXY(10f, 10f));
```

## Converting to VectorXYInt

Converting a floating-point vector to an integer vector is explicit because fractional components are truncated:

```csharp
var vector = new VectorXY(3.8f, -2.4f);
var integerVector = (VectorXYInt)vector;
```

Use the rounding extensions before conversion when truncation is not the intended behavior.
