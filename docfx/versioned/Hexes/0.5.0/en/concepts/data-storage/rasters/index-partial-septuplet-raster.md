# IndexPartialSeptupletRaster

A seven-cell neighborhood with HasMain and HasAdjacent0 through HasAdjacent5 flags. All logical indices are retained, even for absent positions. If Main is outside the source topology, all flags are clear, even if a neighbor lies inside. Use flags to select valid filter inputs.

Sample type: `PartialSeptuplet<VectorXYInt>`.
Components: `Main, Adjacent0 … Adjacent5`.

## Construction and Access

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(8, 6, Layout.OddR);
var source = new HexMapGeometry(topology, new VectorXY(0f, 0f), 1f);
var sampling = source.ToRasterGeometry(pixelsPerApothem: 4f);
var raster = new IndexPartialSeptupletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

No `TryGetValue` method is provided. Check raster bounds before indexing.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster>.
