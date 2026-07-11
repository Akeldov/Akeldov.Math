# VectorXYInt

`VectorXYInt` represents a two-dimensional vector with integer components. Use it for raster resolutions, grid dimensions, discrete offsets, and other values that must remain integral.

## Basic Operations

```csharp
using Akeldov.Math.Spatial2D;

var a = new VectorXYInt(6, 4);
var b = new VectorXYInt(2, 1);

VectorXYInt sum = a + b;
VectorXYInt difference = a - b;
VectorXYInt scaled = a * 2;
VectorXYInt divided = a / 2;

float length = a.Length;
```

`VectorXYInt.Zero` and `VectorXYInt.One` provide the commonly used constant vectors.

Integer division follows C# integer division rules. Dividing `(7, 5)` by `2` produces `(3, 2)`.

## Fractional Scaling

Scaling an integer vector by a floating-point value returns `VectorXY`, preserving the fractional result:

```csharp
var resolution = new VectorXYInt(3, 2);
VectorXY scaled = resolution * 1.5f;
```

## Converting to VectorXY

Conversion from `VectorXYInt` to `VectorXY` is implicit because every integer component can be represented by the floating-point vector API:

```csharp
var resolution = new VectorXYInt(1920, 1080);
VectorXY size = resolution;
```

Conversion in the opposite direction is explicit because it can discard fractional values. See [`VectorXY`](vectorxy.md#converting-to-vectorxyint).
