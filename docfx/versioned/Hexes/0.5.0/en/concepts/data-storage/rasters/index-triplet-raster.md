# IndexTripletRaster

Indices of the main hex and the two hexes meeting it at the closest vertex. Use these indices to retrieve map values for interpolation. The result describes the infinite grid, including indices outside the finite map.

Sample type: `Triplet<VectorXYInt>`.
Components: `Main, Left, Right`.

## Construction and Access

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(8, 6, Layout.OddR);
var source = new HexMapGeometry(topology, new VectorXY(0f, 0f), 1f);
var sampling = source.ToRasterGeometry(pixelsPerApothem: 4f);
var raster = new IndexTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` checks raster bounds; success does not guarantee membership in the finite map.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster>.
