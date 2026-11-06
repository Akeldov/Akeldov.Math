# IndexSeptupletMap

Precomputes each hex index as Main and its six edge-adjacent indices as Adjacent0 through Adjacent5. Use it for repeated neighborhood queries when the caller handles map boundaries or provides padded data.

Value type: `Septuplet<VectorXYInt>`.

## Construction and Access

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var map = new IndexSeptupletMap(topology);
Septuplet<VectorXYInt> neighbors = map[new VectorXYInt(1, 1)];
VectorXYInt main = neighbors.Main;
VectorXYInt adjacent = neighbors.Adjacent0;
```

Adjacent slots follow `HexEdge.Edge0` through `HexEdge.Edge5`; offsets depend on layout and row or column parity. The complete result retains infinite-grid indices, including negative coordinates and indices beyond the topology. Validate a neighbor before indexing a finite value map.

The map owns its precomputed array and exposes read-only indexers. Returned structs do not allow replacing stored neighborhoods. Use [IndexPartialSeptupletMap](index-partial-septuplet-map.md) when boundary membership should be precomputed too.

## Storage and Indexing

Construction time and retained storage grow with `Topology.Count`. Reuse the map when queries share the same source grid. The vector indexer addresses a source hex; the flat indexer uses `y * Topology.Resolution.X + x`. Both require an in-bounds lookup index.

[Maps](../maps.md) · API: <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletMap>.
