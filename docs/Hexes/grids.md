# Grids

Grids are general sampled value containers used by geometry, topology, chromatization, and rasterization APIs.

## `IGrid<TValue>`

- Common contract for sampled grid values.
- Exposes width, height, and count metadata.
- Supports index-based sampled value access.
- Keeps sampled data independent from a specific domain model.

## Shared Grid Behavior

- Flat index mapping.
- Bounds validation.
- Sample count metadata.
- Shared indexing patterns for sampled data.

## Rasterization Integration

- Grids can be converted into raster-friendly sampled data.
- Domain-specific grid rasterization extensions live with their domain APIs.
- RGBA16 rasterization uses grid dimensions and sampled values to produce image data.
