# Working with QRS Coordinates

In this part of the tutorial, you will convert a row-and-column index to a QRS coordinate and back.
This separation lets the application store a rectangular map by `X/Y` index while performing
hex-grid operations in the layout-independent QRS coordinate system.

## Two forms of one index

Add these namespaces at the top of `Program.cs`:

```csharp
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
```

Add the conversion after the `layout` declaration:

```csharp
var storageIndex = new VectorXYInt(3, 2);
VectorQRSInt qrsIndex = storageIndex.ToQRSIndex(layout);
VectorXYInt restoredIndex = qrsIndex.ToXYIndex(layout);

Console.WriteLine($"XY index:  ({storageIndex.X}, {storageIndex.Y})");
Console.WriteLine($"QRS index: ({qrsIndex.Q}, {qrsIndex.R}, {qrsIndex.S})");
Console.WriteLine($"Round trip: {restoredIndex == storageIndex}");
```

For `OddR`, the result is:

```text
XY index:  (3, 2)
QRS index: (2, 2, -4)
Round trip: True
```

<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> stores `Q`, `R`, and the derived `S` component,
always preserving the invariant `Q + R + S = 0`. The two-argument constructor computes `S`
automatically:

```csharp
var direction = new VectorQRSInt(1, 0);
VectorQRSInt adjacentQrsIndex = qrsIndex + direction;

Console.WriteLine(
    $"Q-direction offset: ({adjacentQrsIndex.Q}, {adjacentQrsIndex.R}, {adjacentQrsIndex.S})");
```

A QRS coordinate does not depend on the odd/even row or column offset rule. The `layout` value is
needed only when converting between QRS and the rectangular storage index.

The `storageIndex` and `qrsIndex` variables served the demonstration; later steps may keep or
remove them. Continue with [Creating the Topology](creating-the-topology.md).
