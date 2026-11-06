# IndexSeptupletRaster

The main hex and its six edge neighbors in library edge order. Use this raster for seven-cell filters or cellular rules. Indices outside the finite map are retained. This neighborhood is not an interpolation triangle.

Sample type: `Septuplet<VectorXYInt>`.
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
var raster = new IndexSeptupletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

No `TryGetValue` method is provided. Check raster bounds before indexing.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletRaster>.
