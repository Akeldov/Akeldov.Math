# Handling Partial Boundary Neighborhoods

Use a partial raster when samples can touch or extend beyond a finite map. It retains the same
main-left-right ordering and records explicitly which positions are inside the source topology.

## Switch to partial indices

Replace the `IndexTripletRaster` construction from the previous step with:

```csharp
var indexRaster = new IndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Each sample is now a `PartialTriplet<VectorXYInt>`. Before reading an index from `elevationMap`,
check its matching `HasMain`, `HasLeft`, or `HasRight` property. Do not compare an index with
`VectorXYInt.Zero`: `(0, 0)` is a valid map cell, not an absence marker.

The margin also contains samples with no map cell at all. Such a sample has
`Presence == TripletPresenceFlags.None`; it will become a transparent pixel later.

Partial rasters do not clamp an out-of-range index to the nearest cell. The presence flags keep
the finite-map boundary explicit, which lets the interpolation step decide whether to normalize
the remaining weights or leave the pixel empty.

Continue with [Creating a Barycentric Raster](creating-a-barycentric-raster.md).
