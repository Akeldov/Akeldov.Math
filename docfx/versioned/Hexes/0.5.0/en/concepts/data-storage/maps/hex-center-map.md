# HexCenterMap

Precomputes the world-space center of every hex. Use it when several field samples, distance queries, or partitioning operations reuse the same centers. Unlike a raster, it contains exactly one point per source hex.

Value type: `PointXY`.

## Construction and Access

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var geometry = new HexMapGeometry(topology, new VectorXY(10f, 20f), 2f);
var map = new HexCenterMap(geometry);
PointXY center = map[new VectorXYInt(1, 1)];
```

The geometry constructor accounts for layout, origin, and radius. The topology constructor uses unit radius. Centers are eagerly computed in row-major order.

`HexCenterMap` inherits mutable `HexMap<PointXY>` storage. Changing a stored point does not update `Geometry`; keep the points consistent with it when using geometric algorithms.

## Storage and Indexing

Construction time and retained storage grow with `Topology.Count`. Reuse the map when queries share the same source grid. The vector indexer addresses a source hex; the flat indexer uses `y * Topology.Resolution.X + x`. Both require an in-bounds lookup index.

[Maps](../maps.md) · API: <xref:Akeldov.Math.Hexes.Geometry.HexCenterMap>.
