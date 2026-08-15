# Create and Populate a HexMap

Use <xref:Akeldov.Math.Hexes.HexMap`1> to store one mutable value for each cell of a finite
rectangular hex map. A topology defines the dimensions and layout, while `HexMap<TValue>` stores
the values.

## Create the map

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var map = new HexMap<int>(topology);
```

The constructor creates `topology.Count` cells and initializes them with `default(TValue)`. For
`HexMap<int>`, every cell initially contains `0`.

## Populate and read cells

Iterate over the topology's rows and columns, and address each cell with a `VectorXYInt`:

```csharp
for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        map[new VectorXYInt(x, y)] = x + y * 10;
    }
}

var cell = new VectorXYInt(2, 1);
int flatIndex = cell.Y * topology.Resolution.X + cell.X;

Console.WriteLine($"By XY index: {map[cell]}");
Console.WriteLine($"By flat index {flatIndex}: {map[flatIndex]}");
```

The result is:

```text
By XY index: 12
By flat index 6: 12
```

The `VectorXYInt` indexer is convenient for row-and-column access. The `int` indexer addresses
the same cells in row-major order: `X` changes first, followed by `Y`. The layout changes the
hex-grid interpretation of the indices, but not their storage order.

## Initialize with existing values

If the values are already in row-major order, pass their array to the constructor:

```csharp
var values = new[]
{
     0,  1,  2,  3, // y = 0
    10, 11, 12, 13, // y = 1
    20, 21, 22, 23, // y = 2
};

var initializedMap = new HexMap<int>(topology, values);
```

The array length must equal `topology.Count`. The constructor retains the array without copying
it, so changes through the map are visible through `values`, and vice versa. If shared mutable
storage is not intended, pass a clone: `(int[])values.Clone()`.

Next, [find hex neighbors](find-hex-neighbors.md). For the complete storage and indexing contract,
see [Maps](../../concepts/data-storage/maps.md).
