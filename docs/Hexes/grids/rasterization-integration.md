# Rasterization Integration

Grid APIs support rasterization by exposing sampled values with stable dimensions.

## Grid Conversion

- Grids can be converted into raster-friendly sampled data.
- Domain-specific rasterization extensions live with their domain APIs.
- Rasterization uses grid dimensions and sampled values together.

## RGBA16 Output

- Geometry grids can feed geometry RGBA16 rasterizers.
- Topology grids can feed topology RGBA16 rasterizers.
- Chromatic grids can feed chromatization RGBA16 rasterizers.

## Separation of Concerns

- `IGrid<TValue>` remains domain-neutral.
- Rasterization extensions decide how values map to colors.
