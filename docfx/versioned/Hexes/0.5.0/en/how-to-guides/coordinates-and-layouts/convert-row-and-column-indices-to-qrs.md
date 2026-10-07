# Convert Row and Column Indices to QRS

Use `ToQRSIndex(layout)` when a `VectorXYInt` index from a rectangular map must be converted to
a QRS coordinate that is independent of row or column offsets.

## Convert a storage index

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
var storageIndex = new VectorXYInt(4, 3);

VectorQRSInt qrsIndex = storageIndex.ToQRSIndex(layout);

Console.WriteLine(
    $"QRS: ({qrsIndex.Q}, {qrsIndex.R}, {qrsIndex.S})");
```

The result is:

```text
QRS: (3, 3, -6)
```

<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> is useful for distance, offset, and rotation
calculations because one QRS coordinate identifies the same hex regardless of the odd or even
offset rule.

## Preserve the layout at the storage boundary

A QRS value does not retain its source layout. To recover the original index later, pass the
same `layout` value:

```csharp
VectorXYInt restored = qrsIndex.ToXYIndex(layout);

Console.WriteLine(restored == storageIndex); // True
```

Converting the result with another layout still leaves QRS valid, but produces a different row
and column index. Keep the layout with the topology instead of choosing it again for every call.

`ToQRSIndex` does not check the bounds of a particular map. Negative `VectorXYInt` values and
values outside a resolution are valid on the unbounded coordinate grid.

For the forward operation, see
[Convert QRS to row and column indices](convert-qrs-to-row-and-column-indices.md).
