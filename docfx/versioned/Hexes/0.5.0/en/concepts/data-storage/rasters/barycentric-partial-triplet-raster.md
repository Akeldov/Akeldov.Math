# BarycentricPartialTripletRaster

Pairs with IndexPartialTripletRaster. HasMain, HasLeft, and HasRight identify weights whose hexes belong to the source topology. Missing positions contain zero; remaining weights are not renormalized. Their sum can be less than one, so choose a boundary policy before interpolating.

Sample type: `PartialTriplet<float>`.
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
var raster = new BarycentricPartialTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` returns `false` outside the raster or when no source-map position is present.

See [Rasters](../rasters.md) for sampling geometry, storage, and shared boundary rules.

API: <xref:Akeldov.Math.Hexes.Topology.BarycentricPartialTripletRaster>.
