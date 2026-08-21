# Get a Hex Chromatic Index

Use `GetChromaticClass` to assign a hex index to one of three classes: `0`, `1`, or `2`. Hexes that
share an edge always have different classes, so the result can identify independent processing
passes or stable interpolation channels.

## Classify an index

Import the chromatization extensions and pass the same layout that defines the source topology:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var hexIndex = new VectorXYInt(2, 1);
int chromaticIndex = hexIndex.GetChromaticClass(topology.Layout);

Console.WriteLine(chromaticIndex); // 1
```

The returned number is a class identifier rather than an image color. Assign any application
meaning or palette to classes `0`, `1`, and `2` as needed.

Always use the topology's actual layout. The same `(X, Y)` index can receive a different class in
row-offset and column-offset layouts. Passing an unsupported `Layout` value throws
`ArgumentOutOfRangeException`.

## Use a layout-specific method

When the layout is fixed by the surrounding code, call its dedicated extension:

```csharp
int chromaticIndex = hexIndex.GetOddRChromaticClass();
```

The available methods are `GetOddRChromaticClass`, `GetEvenRChromaticClass`,
`GetOddQChromaticClass`, and `GetEvenQChromaticClass`. They return the same value as
`GetChromaticClass` with the corresponding layout.

## Apply the result correctly

The class depends only on the logical index and layout. Map dimensions, values, hex radius, and
world-space origin do not affect it. Negative indices are valid inputs and still produce a value
from `0` through `2`, so the method also classifies the implied infinite hex lattice.

The three-color guarantee applies to direct edge neighbors. Cells in the same class do not share
an edge, but they may still influence one another in algorithms whose neighborhood extends more
than one step.

For repeated lookups across a finite topology, continue with
[Create a Chromatic Map](create-a-chromatic-map.md). The invariant and its use with triplet rasters
are described in [Chromatization](../../concepts/spatial-algorithms/chromatization.md).
