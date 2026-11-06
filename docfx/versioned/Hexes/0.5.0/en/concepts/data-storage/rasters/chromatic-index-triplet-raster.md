# ChromaticIndexTripletRaster

Chromatic classes 0, 1, and 2 in geometric triplet order. These are class labels, not map indices or colors. A complete triplet contains all three classes. Use it to associate geometric positions with class-based data. Finite-map boundaries do not clip the result.

Sample type: `Triplet<byte>`.
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
var raster = new ChromaticIndexTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` checks raster bounds; success does not guarantee membership in the finite map.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexTripletRaster>.
