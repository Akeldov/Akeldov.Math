# Rotate Hex Coordinates

Use `Rotate(SixfoldAngle)` for an exact grid rotation in 60-degree steps, or
`Rotate(float angleRad)` for an arbitrary angle and a fractional result. Positive angles rotate
counterclockwise.

## Rotate exactly by 60 degrees

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;

var index = new VectorQRSInt(q: 2, r: -5);

VectorQRSInt rotated = index.Rotate(SixfoldAngle.Deg60);

Console.WriteLine(
    $"Rotated: ({rotated.Q}, {rotated.R}, {rotated.S})");
```

The result is:

```text
Rotated: (5, -3, -2)
```

<xref:Akeldov.Math.Hexes.SixfoldAngle> contains six values from `Deg0` through `Deg300`. Rotation
permutes and negates QRS components, so the integer index and `Q + R + S = 0` invariant are
preserved without rounding error.

## Rotate around another hex

`Rotate` uses the QRS origin as its center. To use another center, translate the index, rotate
the offset, and translate it back:

```csharp
var pivot = new VectorQRSInt(q: 1, r: -1);
var point = new VectorQRSInt(q: 3, r: -2);

VectorQRSInt aroundPivot =
    (point - pivot).Rotate(SixfoldAngle.Deg120) + pivot;
```

## Rotate by an arbitrary angle

The numeric overload accepts radians and returns a fractional
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS>:

```csharp
VectorQRS rotatedBy30Degrees = index.Rotate(MathF.PI / 6f);

VectorQRSInt nearestHex =
    rotatedBy30Degrees.ToQRSIndex(Layout.OddR);
```

Do not pass `60f` for a 60-degree rotation: that means 60 radians. Use `SixfoldAngle.Deg60` or
`MathF.PI / 3f`.

QRS rotation is independent of layout. The final example needs `Layout` only to select the
nearest integer hex after a fractional rotation.

For more information, see
[Rotations and Transformations](../../concepts/fundamentals/rotations-and-transformations.md).
