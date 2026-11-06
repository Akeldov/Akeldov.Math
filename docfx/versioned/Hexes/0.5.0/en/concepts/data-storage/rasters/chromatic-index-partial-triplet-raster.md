# ChromaticIndexPartialTripletRaster

Chromatic classes with HasMain, HasLeft, and HasRight flags. Positions outside the source topology are absent and contain zero. Class 0 is valid, so determine absence only from flags. Use this raster for class lookup restricted to the finite map.

Sample type: `PartialTriplet<byte>`.
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
var raster = new ChromaticIndexPartialTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

No `TryGetValue` method is provided. Check raster bounds before indexing.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexPartialTripletRaster>.
