# ChromaticBarycentricPartialTripletRaster

Class-ordered barycentric weights with HasIndex0, HasIndex1, and HasIndex2 flags. Weights and flags are reordered together. Missing positions contain zero; surviving weights are not renormalized. Use it for class-based blending restricted to the source topology.

Sample type: `PartialChromaticTriplet<float>`.
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
var raster = new ChromaticBarycentricPartialTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` checks raster bounds; success does not guarantee membership in the finite map. It can return `true` with `Presence == None`.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster>.
