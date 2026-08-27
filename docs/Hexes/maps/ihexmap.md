# `IHexMap<TValue>`

`IHexMap<TValue>` is the common contract for hex-indexed value maps.

## Metadata

- `Topology`, including width, height, and layout.

## Access

- Coordinate-based value access.
- Flat hex-indexed lookup.
- Bounds-aware implementations.

## Role

- Keeps map consumers independent from specific value domains.
- Allows topology, geometry, chromatization, and partitioning maps to share a common shape.
- Provides read-only access without promising immutable or snapshot state.

Mutable implementations may change while they are observed through `IHexMap<TValue>`. Algorithms
that need to write values use the separate mutable-map capability rather than assuming every map
implementation owns writable storage.

## Rasterization

- `IHexMap<TValue>` can be rasterized with an explicit `RasterGeometry`.
- The raster grid resolution must match the map width and height.
- This rasterization maps each hex-map value to one raster value; geometry-aware hex-field rasterizers can draw hex cells at arbitrary pixel density.
