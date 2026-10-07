# Round Fractional QRS Coordinates

Use `VectorQRS.ToQRSIndex(layout)` when a continuous QRS coordinate must be mapped to the nearest
integer hex.

## Round to a valid QRS index

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;

var fractional = new VectorQRS(q: 1.2f, r: -2.3f);

VectorQRSInt nearest = fractional.ToQRSIndex(Layout.OddR);

Console.WriteLine(
    $"Nearest: ({nearest.Q}, {nearest.R}, {nearest.S})");
```

The result is:

```text
Nearest: (1, -2, 1)
```

The method rounds the three cube-coordinate components, then corrects the component with the
largest error to preserve `Q + R + S = 0`. The `Layout` argument determines how boundary cases
are resolved for the grid orientation; odd and even variants of one orientation produce the
same QRS result.

## Do not cast when finding the nearest hex

```csharp
VectorQRSInt truncated = (VectorQRSInt)fractional;
```

The explicit conversion truncates `Q` and `R` toward zero. It does not perform cube rounding and
can select a different hex. Use it only when truncation itself is part of the algorithm.

`ToQRSIndex` rejects `NaN`, infinity, unknown layouts, and results outside the `Int32` range.
After rounding, use `ToXYIndex(layout)` when you need a row and column index for rectangular
storage.

For details about coordinate types, see
[QRS Coordinates](../../concepts/fundamentals/coordinate-systems/qrs-coordinates.md).
