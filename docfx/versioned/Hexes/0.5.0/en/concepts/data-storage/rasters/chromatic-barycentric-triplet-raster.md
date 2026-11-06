# ChromaticBarycentricTripletRaster

Barycentric weights reordered by chromatic class. Each component refers to a class rather than Main, Left, or Right. Use it for blending three class-based channels. The weights sum to approximately one and are not clipped to the finite map.

Sample type: `ChromaticTriplet<float>`.
Components: `Index0, Index1, Index2`.

## Construction and Access

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(8, 6, Layout.OddR);
var source = new HexMapGeometry(topology, new VectorXY(0f, 0f), 1f);
var sampling = source.ToRasterGeometry(pixelsPerApothem: 4f);
var raster = new ChromaticBarycentricTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` checks raster bounds; success does not guarantee membership in the finite map.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster>.
