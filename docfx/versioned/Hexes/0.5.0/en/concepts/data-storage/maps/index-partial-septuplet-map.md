# IndexPartialSeptupletMap

Precomputes Main and six adjacent indices together with flags indicating membership in the finite source topology. Use it to traverse valid neighbors repeatedly without recalculating boundary checks.

Value type: `PartialSeptuplet<VectorXYInt>`.

## Construction and Access

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var map = new IndexPartialSeptupletMap(topology);
PartialSeptuplet<VectorXYInt> neighbors = map[VectorXYInt.Zero];
if (neighbors.HasAdjacent0)
{
    VectorXYInt adjacent = neighbors.Adjacent0;
}
```

For every valid lookup, `HasMain` is true. `HasAdjacent0` through `HasAdjacent5` identify valid neighbors in edge order. Absent positions still retain their logical indices: consult flags rather than payload values. The lookup index itself must be inside the map.

The map owns its precomputed storage and has read-only indexers. A returned value is a struct, not a mutable view into that storage. See [Neighborhood values](../neighborhood-values.md) for presence flags and direct neighbor-value sampling.

## Storage and Indexing

Construction time and retained storage grow with `Topology.Count`. Reuse the map when queries share the same source grid. The vector indexer addresses a source hex; the flat indexer uses `y * Topology.Resolution.X + x`. Both require an in-bounds lookup index.

[Maps](../maps.md) · API: <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletMap>.
