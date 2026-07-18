# `ISpatialRaster<TValue>`

Hex raster types implement the Spatial2D `ISpatialRaster<TValue>` contract.

## Metadata

- Raster geometry.
- Two-dimensional resolution.

## Access

- Flat sample indexing.
- Two-dimensional sample indexing.
- Safe lookup through `TryGetValue`.

## Role

- Carries spatial bounds together with sampled values.
- Allows common raster mapping and imaging extensions to consume Hexes rasters.

## Rasterization

- `ISpatialRaster<TValue>` already contains its `RasterGeometry`.
- `MapValues` creates a new spatial raster while preserving that geometry.
- Imaging extensions can save mapped rasters in supported image formats.
