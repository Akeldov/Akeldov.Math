# Rasterization Integration

Hex raster APIs expose sampled values together with stable spatial geometry.

## Value Mapping

- `MapValues` converts values while preserving raster geometry.
- Domain-specific mapping functions decide how values become colors or other samples.
- Raster indexing uses resolution and sampled values together.

## RGBA16 Output

- Geometry rasters can be mapped to RGBA16 colors.
- Topology rasters can be mapped to RGBA16 colors.
- Chromatic rasters can be mapped to RGBA16 colors.

## Separation of Concerns

- `ISpatialRaster<TValue>` remains domain-neutral.
- Rasterization extensions decide how values map to colors.
