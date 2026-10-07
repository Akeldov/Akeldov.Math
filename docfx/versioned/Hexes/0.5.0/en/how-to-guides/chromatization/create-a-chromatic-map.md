# Create a Chromatic Map

Use <xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap> to precompute one three-color class
for every cell in a finite hex topology. The map is useful when the same classes are read repeatedly,
processed in separate passes, or rasterized for visualization.

## Create a map from a topology

Pass the source topology to the constructor:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var chromaticMap = new ChromaticIndexMap(topology);
```

Construction calculates one `byte` value—`0`, `1`, or `2`—for every topology cell. The map retains
the same resolution and layout through its `Topology` property. Because it implements the read-only
`ISpatialHexMap<byte>` contract, its classes can be read but not replaced.

The topology-only constructor supplies default world placement with unit hex radius. That geometry
does not affect the classes; it only allows the result to participate in spatial APIs.

## Read stored classes

Read a class by its `(X, Y)` index or by a zero-based row-major flat index:

```csharp
var index = new VectorXYInt(2, 1);

byte byCoordinates = chromaticMap[index];
int flatIndex = index.Y * chromaticMap.Topology.Resolution.X + index.X;
byte byFlatIndex = chromaticMap[flatIndex];

Console.WriteLine(byCoordinates); // 1
Console.WriteLine(byCoordinates == byFlatIndex); // True
```

The coordinate indexer validates both components and throws `IndexOutOfRangeException` outside the
finite topology. Unlike `GetChromaticClass`, `ChromaticIndexMap` does not expose classes for negative
or otherwise out-of-map indices.

## Process one class at a time

Run three passes when cells changed together must not share an edge:

```csharp
for (byte classIndex = 0; classIndex < 3; classIndex++)
{
    for (int y = 0; y < topology.Resolution.Y; y++)
    {
        for (int x = 0; x < topology.Resolution.X; x++)
        {
            var index = new VectorXYInt(x, y);

            if (chromaticMap[index] != classIndex)
                continue;

            Console.WriteLine($"Pass {classIndex}: {index}");
        }
    }
}
```

Within one pass, no two selected cells are direct edge neighbors. This does not make the pass
independent when an operation reads or writes a neighborhood extending more than one edge.

## Preserve an existing spatial geometry

Construct the map from `HexMapGeometry` when its world-space radius and origin must match another
map or raster:

```csharp
using Akeldov.Math.Hexes.Geometry;

var geometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

var spatialChromaticMap = new ChromaticIndexMap(geometry);

Console.WriteLine(spatialChromaticMap.Geometry == geometry); // True
```

The classes still depend only on index and layout, while `Geometry` controls later spatial
sampling. Continue with [Create a Chromatic Raster](create-a-chromatic-raster.md) to sample
chromatic data on a rectangular grid, or see
[Get a Hex Chromatic Index](get-a-hex-chromatic-index.md) for one-off classification. The complete
model is described in [Chromatization](../../concepts/spatial-algorithms/chromatization.md).
