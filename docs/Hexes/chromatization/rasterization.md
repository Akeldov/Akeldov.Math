# Chromatic Rasterization

Chromatic rasterization maps chromatic values into RGBA16 output.

## Data Sources

- `ChromaticIndexMap`.
- `ChromaticIndexTripletRaster`.
- `ChromaticIndexPartialTripletRaster`.

## Color Mapping

- Map chromatic index values to colors.
- Use caller-provided color mapping where supported.
- Preserve layout-aware raster output.

## Output

- Produce RGBA16 raster data.
- Use raster resolution and geometry to place sampled values.
