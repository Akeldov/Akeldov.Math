# Vectors

`VectorXY` represents a two-dimensional vector with single-precision floating-point components. `VectorXYInt` represents the same concept with integer components.

## Basic Operations

```csharp
using System;
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
var vector = new VectorXY(10f, 0f);
var rotated = vector.Rotate(MathF.PI / 2f);
var clamped = rotated.Clamp(new VectorXY(0f, 0f), new VectorXY(10f, 10f));
```
