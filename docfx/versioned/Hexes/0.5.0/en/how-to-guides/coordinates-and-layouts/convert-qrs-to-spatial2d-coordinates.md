# Convert QRS to Spatial2D Coordinates

Use `GetHexOffset` for an integer QRS index or `ToVectorXY` for fractional QRS when a logical
coordinate must be placed in continuous Spatial2D space.

## Get the center of an integer hex

Set the radius, layout, and center of the zero hex:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
const float hexRadius = 2f;
var zeroHexCenter = new VectorXY(10f, 20f);
var qrsIndex = new VectorQRSInt(q: 2, r: 1);

VectorXY offset = qrsIndex.GetHexOffset(hexRadius, layout);
VectorXY center = zeroHexCenter + offset;
var centerPoint = new PointXY(center.X, center.Y);

Console.WriteLine(FormattableString.Invariant(
    $"Offset: ({offset.X:F3}, {offset.Y:F3})"));
Console.WriteLine(FormattableString.Invariant(
    $"Center: ({centerPoint.X:F3}, {centerPoint.Y:F3})"));
```

The approximate result is:

```text
Offset: (8.660, 3.000)
Center: (18.660, 23.000)
```

`GetHexOffset` returns a `VectorXY`: the offset of the selected hex center from the zero hex
center. Add it to your `zeroHexCenter`; do not interpret the origin as a map corner.

## Convert a fractional QRS coordinate

<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> converts into the unit-radius grid basis. Multiply
the result by the actual radius:

```csharp
var fractional = new VectorQRS(q: 2.25f, r: 0.5f);

VectorXY fractionalOffset =
    fractional.ToVectorXY(layout) * hexRadius;
VectorXY fractionalPosition = zeroHexCenter + fractionalOffset;
```

`OddR` and `EvenR` use the same spatial QRS basis with pointy-top hexes; `OddQ` and `EvenQ` use
the same basis with flat-top hexes. The odd or even offset rule changes storage indices, but not
the position of a single QRS coordinate.

For the reverse cell lookup, see
[Find the nearest hex to a point](find-the-nearest-hex-to-a-point.md).
