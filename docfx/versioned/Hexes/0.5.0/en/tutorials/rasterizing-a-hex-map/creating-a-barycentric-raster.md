# Creating a Barycentric Raster

The index raster identifies the three surrounding hex centers. A barycentric raster supplies the
matching interpolation weights for those centers.

## Reuse the sampling geometry

Add this code immediately after constructing `indexRaster`:

```csharp
var barycentricRaster = new BarycentricPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Both rasters must use the same `HexMapGeometry` and the same `RasterGeometry`. Equal resolutions
alone are insufficient: origins and world-space sizes must also match. With shared geometries,
flat index `i` describes the same pixel sample and the same main-left-right positions in both
rasters.

For an interior sample, the `Main`, `Left`, and `Right` weights sum to approximately `1`. At the
finite boundary, the partial raster preserves only the present positions and their original
weights. It deliberately does not renormalize them; the next step will do that while combining
the weights with map values.

Constructing these rasters performs all spatial classification eagerly. Reuse them when rendering
the same geometry with updated map values.

Continue with [Mapping Map Values to Pixels](mapping-map-values-to-pixels.md).
