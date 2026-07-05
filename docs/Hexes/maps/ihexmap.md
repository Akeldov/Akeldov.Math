# `IHexMap<TValue>`

`IHexMap<TValue>` is the common contract for hex-indexed value maps.

## Metadata

- Width.
- Height.

## Access

- Coordinate-based value access.
- Flat hex-indexed lookup.
- Bounds-aware implementations.

## Role

- Keeps map consumers independent from specific value domains.
- Allows topology, geometry, chromatization, and partitioning maps to share a common shape.

## Rasterization

- `IHexMap<TValue>` can be rasterized with an explicit `SpatialRasterGrid`.
- The raster grid resolution must match the map width and height.
- This rasterization maps each hex-map value to one raster value; geometry-aware hex-field rasterizers can draw hex cells at arbitrary pixel density.
