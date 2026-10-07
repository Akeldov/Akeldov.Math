# Creating an Index Raster

An index raster answers which three hex centers surround each pixel sample. It stores semantic
lookup data; it does not draw an image yet.

## Define one sampling grid

Add this code after populating `elevationMap`:

```csharp
const float PixelsPerApothem = 32f;

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: PixelsPerApothem,
    margin: mapGeometry.Radius * 0.5f);

var indexRaster = new IndexTripletRaster(mapGeometry, rasterGeometry);
```

`RasterGeometry` fixes the origin, world-space size, and pixel resolution for the remainder of the
tutorial. A density of 32 pixels per apothem gives smooth output, while the half-radius margin
makes the transparent area around the map visible.

For every raster cell, `IndexTripletRaster` stores a `Triplet<VectorXYInt>` ordered as `Main`,
`Left`, and `Right`. `Main` is the containing or nearest hex. `Left` and `Right` are the other two
hexes that meet it at the closest vertex.

## Why the complete raster is not enough

The complete raster describes the infinite hex grid implied by the geometry. Near the outer
boundary, some stored indices therefore fall outside the finite 9×7 topology. This is useful for
geometric calculations, but indexing `elevationMap` with one of those values would throw an
`IndexOutOfRangeException`.

The next step replaces this raster with its boundary-aware counterpart. Continue with
[Handling Partial Boundary Neighborhoods](handling-partial-boundary-neighborhoods.md).
