# ChromaticIndexMap

Precomputes a three-color class for every hex. Each value is 0, 1, or 2; edge-adjacent hexes have different classes. These bytes are class labels, not map indices or display colors.

Value type: `byte`.

## Construction and Access

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var map = new ChromaticIndexMap(topology);
byte chromaticClass = map[new VectorXYInt(1, 1)];
```

Use the class to select one of three channels or to associate hex data with chromatic interpolation. Classes depend on topology and layout. The finite boundary does not create missing slots: each valid hex has one class.

Construction fills internal storage once; indexers are read-only. Class 0 is a valid result and must not be interpreted as absence. See [Chromatization](../../spatial-algorithms/chromatization.md) for the pattern and [Rasters](../rasters.md) for spatially sampled class triplets.

## Storage and Indexing

Construction time and retained storage grow with `Topology.Count`. Reuse the map when queries share the same source grid. The vector indexer addresses a source hex; the flat indexer uses `y * Topology.Resolution.X + x`. Both require an in-bounds lookup index.

[Maps](../maps.md) · API: <xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>.
