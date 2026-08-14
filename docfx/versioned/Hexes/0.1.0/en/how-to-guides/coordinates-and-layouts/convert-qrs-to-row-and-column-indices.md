# Convert QRS to Row and Column Indices

Use `ToXYIndex(layout)` when an integer QRS coordinate must be used as the column and row index
of rectangular storage.

## Convert the index

Import the Hexes, QRS, and Spatial2D namespaces:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
```

Pass the same layout as the map uses:

```csharp
Layout layout = Layout.OddR;
var qrsIndex = new VectorQRSInt(q: 3, r: 3);

VectorXYInt storageIndex = qrsIndex.ToXYIndex(layout);

Console.WriteLine($"XY: ({storageIndex.X}, {storageIndex.Y})");
```

The result is:

```text
XY: (4, 3)
```

For `OddR`, the odd row `R = 3` is shifted to the right, so column `X` is `4`. Component
`S = -6` follows from the QRS invariant and is not passed separately to the conversion.

## Verify with the reverse conversion

To verify that the parameters agree, convert the result back:

```csharp
VectorQRSInt restored = storageIndex.ToQRSIndex(layout);

Console.WriteLine(restored == qrsIndex); // True
```

`ToXYIndex` and `ToQRSIndex` are exact inverses for positive and negative indices when both
calls use the same layout.

The method operates on an unbounded logical grid. Before accessing a `HexMap<T>`, separately
check that `storageIndex.X` and `storageIndex.Y` are within the topology resolution.

For the reverse operation, see
[Convert row and column indices to QRS](convert-row-and-column-indices-to-qrs.md).
