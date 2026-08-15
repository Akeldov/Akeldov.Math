# Get a Ring of a Given Radius

Hexes 0.1.0 has no dedicated ring method. For a finite topology, enumerate its valid indices and
keep the cells whose QRS grid distance from the center is exactly the requested radius.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 5,
    height: 5,
    layout: Layout.OddR);
var center = new VectorXYInt(2, 2);

List<VectorXYInt> ring = GetRing(topology, center, radius: 2);

Console.WriteLine(string.Join(", ", ring));

static List<VectorXYInt> GetRing(
    HexMapTopology topology,
    VectorXYInt center,
    int radius)
{
    if (radius < 0)
        throw new ArgumentOutOfRangeException(nameof(radius));

    if (center.X < 0 || center.X >= topology.Resolution.X ||
        center.Y < 0 || center.Y >= topology.Resolution.Y)
    {
        throw new ArgumentOutOfRangeException(nameof(center));
    }

    var result = new List<VectorXYInt>();
    VectorQRSInt centerQrs = center.ToQRSIndex(topology.Layout);

    for (int y = 0; y < topology.Resolution.Y; y++)
    {
        for (int x = 0; x < topology.Resolution.X; x++)
        {
            var index = new VectorXYInt(x, y);
            VectorQRSInt indexQrs = index.ToQRSIndex(topology.Layout);

            long distance = Math.Max(
                Math.Abs((long)indexQrs.Q - centerQrs.Q),
                Math.Max(
                    Math.Abs((long)indexQrs.R - centerQrs.R),
                    Math.Abs((long)indexQrs.S - centerQrs.S)));

            if (distance == radius)
                result.Add(index);
        }
    }

    return result;
}
```

The method returns a new, mutable list in row-major order:

```text
(1, 0), (2, 0), (3, 0), (0, 1), (3, 1), (0, 2), (4, 2), (0, 3), (3, 3), (1, 4), (2, 4), (3, 4)
```

`radius` is a grid distance measured in edge-adjacent steps, not a geometric hex size. Radius
`0` returns only the center. A complete positive-radius ring contains `6 * radius` cells, but the
scan clips the result to the topology, so a center near an edge or a radius larger than the map can
produce a shorter or empty list. A negative radius or an out-of-bounds center throws
`ArgumentOutOfRangeException`.

See [QRS Coordinates](../../concepts/fundamentals/coordinate-systems/qrs-coordinates.md) for the
distance formula, [Topology](../../concepts/hex-grid-model/topology.md) for finite-map bounds, and
[Find Hex Neighbors](find-hex-neighbors.md) for one-step adjacency.
