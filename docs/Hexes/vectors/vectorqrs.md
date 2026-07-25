# VectorQRS

`VectorQRS` represents a fractional vector in the QRS cube-coordinate plane. Use it for continuous hex-grid positions, interpolation, scaling, and transformations that can produce non-integral coordinates.

The type stores `Q` and `R` as `float` values and derives `S` so that `Q + R + S = 0`.

## Construction and Components

```csharp
var vector = new VectorQRS(1.5f, -0.5f);

float q = vector.Q; // 1.5
float r = vector.R; // -0.5
float s = vector.S; // -1
```

`S` is not supplied separately because only two components are independent.

The predefined values are:

```csharp
VectorQRS zero = VectorQRS.Zero; // (0, 0, 0)
VectorQRS one = VectorQRS.One;   // (1, 1, -2)
```

## Arithmetic

Arithmetic operates component-wise while the constructor keeps the derived `S` component consistent:

```csharp
var a = new VectorQRS(2f, -1f);
var b = new VectorQRS(0.5f, 1f);

VectorQRS sum = a + b;        // (2.5, 0, -2.5)
VectorQRS difference = a - b; // (1.5, -2, 0.5)
VectorQRS scaledA = a * 2f;   // (4, -2, -2)
VectorQRS scaledB = 2f * a;   // (4, -2, -2)
VectorQRS divided = a / 2f;   // (1, -0.5, -0.5)
```

Division follows floating-point rules. Dividing by zero can therefore produce infinite or NaN components.

## Equality

`VectorQRS` implements `IEquatable<VectorQRS>`. Equality compares the independent `Q` and `R` components exactly:

```csharp
var a = new VectorQRS(1f, -2f);
var b = new VectorQRS(1f, -2f);

bool equal = a == b;       // true
bool different = a != b;  // false
```

## Conversion to and from VectorQRSInt

Conversion from [`VectorQRSInt`](vectorqrsint.md) is implicit and exact:

```csharp
var integer = new VectorQRSInt(2, -1);
VectorQRS fractional = integer;
```

Explicit conversion to `VectorQRSInt` truncates `Q` and `R` toward zero:

```csharp
var fractional = new VectorQRS(2.8f, -1.4f);
var truncated = (VectorQRSInt)fractional; // (2, -1, -1)
```

To select the nearest hex rather than truncate components, use layout-aware discretization:

```csharp
VectorQRSInt nearest = fractional.ToQRSIndex(Layout.OddR);
```

See [Discretization](discretization.md) for rounding behavior and validation.

## Coordinate Conversion

`ToVectorXY` maps QRS coordinates onto the continuous Spatial2D axes of a unit-radius hex grid:

```csharp
VectorXY offset = new VectorQRS(1f, 0f).ToVectorXY(Layout.OddR);
```

The inverse conversion is available on `VectorXY`:

```csharp
VectorQRS qrs = offset.ToVectorQRS(Layout.OddR);
```

Odd and even layouts of the same orientation share the same continuous axes. See [Coordinate Conversions](coordinate-conversions.md) for the QRS basis diagrams.

## Rotation

Arbitrary rotations use radians and return `VectorQRS`:

```csharp
VectorQRS rotated = new VectorQRS(1f, 0f).Rotate(MathF.PI / 3f);
```

Exact hex-symmetry rotations use `SixfoldAngle`:

```csharp
VectorQRS rotated60 = new VectorQRS(1f, 0f).Rotate(SixfoldAngle.Deg60);
```

Both forms rotate counterclockwise in the hex coordinate plane. Non-finite arbitrary angles and unsupported `SixfoldAngle` values are rejected.

## Binary Serialization

Binary helpers write and read `Q` followed by `R` as two 32-bit floating-point values:

```csharp
using var stream = new MemoryStream();

using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
    writer.Write(new VectorQRS(1.5f, -0.5f));

stream.Position = 0;

using var reader = new BinaryReader(stream);
VectorQRS restored = reader.ReadVectorQRS();
```
