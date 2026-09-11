# Chromatizing a Hex Map

In this tutorial, you will assign every cell of a hex map to one of three stable classes. Hexes in
the same class never share an edge, so the classes can organize operations into three passes in
which directly adjacent cells are not changed together.

The finished console example prints this pattern for a `5` by `4` `OddR` map:

```text
0 1 2 0 1
2 0 1 2 0
0 1 2 0 1
2 0 1 2 0
```

Complete [Creating a Project](creating-a-hex-map/creating-a-project.md) first, or use an existing
project that references `Akeldov.Math.Hexes`.

## Create the topology and chromatic map

Create a topology, then pass it to
<xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 5,
    height: 4,
    layout: Layout.OddR);

var chromaticMap = new ChromaticIndexMap(topology);
```

Construction calculates one `byte` class—`0`, `1`, or `2`—for every cell. The class depends on
the cell index and layout, not on values stored in another map.

## Print the pattern

Read the class at each row-and-column index:

```csharp
for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        byte chromaticClass = chromaticMap[new VectorXYInt(x, y)];
        Console.Write($"{chromaticClass} ");
    }

    Console.WriteLine();
}
```

Every horizontal pair has different classes. The offset between consecutive rows also ensures
that cells touching across those rows have different classes. The same invariant holds for
`EvenR`, `OddQ`, and `EvenQ`, although their printed patterns differ.

## Group cells into three passes

Collect the finite map indices by class:

```csharp
var passes = new[]
{
    new List<VectorXYInt>(),
    new List<VectorXYInt>(),
    new List<VectorXYInt>()
};

for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        passes[chromaticMap[index]].Add(index);
    }
}

for (int classIndex = 0; classIndex < passes.Length; classIndex++)
{
    Console.WriteLine($"Pass {classIndex}");

    foreach (VectorXYInt index in passes[classIndex])
        Console.WriteLine($"  {index}");
}
```

Within one pass, no two indices are direct edge neighbors. This is useful for in-place local
updates when adjacent cells must not be written at the same time. Parallel execution still
requires caller-owned storage that supports concurrent writes to distinct cells.

The guarantee covers only immediate edge neighbors. If an operation reads or changes cells two
or more steps away, cells in one class can still affect one another. Use a wider partitioning
scheme or a separate output map for such operations.

## Classify one index without a map

For occasional queries, calculate the class directly instead of precomputing the finite map:

```csharp
var index = new VectorXYInt(3, 2);
int chromaticClass = index.GetChromaticClass(topology.Layout);

Console.WriteLine(chromaticClass); // 0
```

`GetChromaticClass` also accepts negative and out-of-map indices because it classifies the implied
infinite hex lattice. The `ChromaticIndexMap` indexer, by contrast, accepts only indices inside its
finite topology.

You now have a repeatable three-pass partition of the map. See the
[Chromatization concept](../concepts/spatial-algorithms/chromatization.md) for the mathematical
invariant and triplet ordering rules. Continue with
[Create a Chromatic Raster](../how-to-guides/chromatization/create-a-chromatic-raster.md) when you
need class-ordered interpolation weights on a rectangular sampling grid.

For a worked RGB example, continue with
[Chromatization and Barycentric Interpolation](chromatic-barycentric-interpolation.md).
