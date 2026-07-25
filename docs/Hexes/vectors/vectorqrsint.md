# VectorQRSInt

`VectorQRSInt` represents an integer vector in the QRS cube-coordinate plane. Use it for discrete hex indexes, neighbor offsets, storage-coordinate conversion, and exact rotations in 60-degree increments.

The type stores integer `Q`, `R`, and `S` components while enforcing `Q + R + S = 0`.

## Construction and Components

Construct a vector from its independent `Q` and `R` components:

```csharp
var vector = new VectorQRSInt(2, -1);

int q = vector.Q; // 2
int r = vector.R; // -1
int s = vector.S; // -1
```

You can also supply all three components. The constructor rejects values whose sum is not zero:

```csharp
var vector = new VectorQRSInt(2, -1, -1);
```

The predefined values are:

```csharp
VectorQRSInt zero = VectorQRSInt.Zero; // (0, 0, 0)
VectorQRSInt one = VectorQRSInt.One;   // (1, 1, -2)
```

## Arithmetic

Integer arithmetic preserves integral components and checks overflow:

```csharp
var a = new VectorQRSInt(2, -1);
var b = new VectorQRSInt(1, 1);

VectorQRSInt sum = a + b;        // (3, 0, -3)
VectorQRSInt difference = a - b; // (1, -2, 1)
VectorQRSInt scaledA = a * 2;    // (4, -2, -2)
VectorQRSInt scaledB = 2 * a;    // (4, -2, -2)
VectorQRSInt divided = a / 2;    // (1, 0, -1)
```

Division follows C# integer-division rules and truncates toward zero. Division by zero throws `DivideByZeroException`.

## Equality

`VectorQRSInt` implements `IEquatable<VectorQRSInt>`. Equality compares the independent `Q` and `R` components:

```csharp
var a = new VectorQRSInt(2, -1);
var b = new VectorQRSInt(2, -1, -1);

bool equal = a == b;       // true
bool different = a != b;  // false
```

## Conversion to and from VectorQRS

Conversion to [`VectorQRS`](vectorqrs.md) is implicit:

```csharp
var integer = new VectorQRSInt(2, -1);
VectorQRS fractional = integer;
```

Conversion from `VectorQRS` is explicit because it truncates fractional `Q` and `R` components:

```csharp
var fractional = new VectorQRS(2.8f, -1.4f);
var truncated = (VectorQRSInt)fractional; // (2, -1, -1)
```

Use `VectorQRS.ToQRSIndex(layout)` when the nearest hex is required instead.

## Offset-Storage Indexes

QRS indexes are independent of row or column storage layout. Convert them to `VectorXYInt` only at the storage boundary:

```csharp
var qrsIndex = new VectorQRSInt(2, -1);

VectorXYInt storageIndex = qrsIndex.ToXYIndex(Layout.OddR);
VectorQRSInt restored = storageIndex.ToQRSIndex(Layout.OddR);
```

Use the same layout in both directions. See [Coordinate Conversions](coordinate-conversions.md) for the difference between row- and column-oriented layouts.

## Rotation

Rotating by a `SixfoldAngle` preserves integer components:

```csharp
VectorQRSInt rotated = new VectorQRSInt(1, 0).Rotate(SixfoldAngle.Deg60);
```

An arbitrary radian angle can produce fractional coordinates and therefore returns `VectorQRS`:

```csharp
VectorQRS rotated = new VectorQRSInt(1, 0).Rotate(MathF.PI / 4f);
```

Both forms rotate counterclockwise. Non-finite arbitrary angles and unsupported `SixfoldAngle` values are rejected.

## Binary Serialization

Binary helpers write and read `Q` followed by `R` as two 32-bit integers. `S` is reconstructed when reading:

```csharp
using var stream = new MemoryStream();

using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
    writer.Write(new VectorQRSInt(2, -1));

stream.Position = 0;

using var reader = new BinaryReader(stream);
VectorQRSInt restored = reader.ReadVectorQRSInt();
```
