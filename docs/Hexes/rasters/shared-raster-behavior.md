# Shared Raster Behavior

Shared raster behavior describes how sampled values are indexed and validated.

## Indexing

- Use flat sample indexes.
- Preserve width and height metadata.
- Map sampled positions consistently across raster implementations.

## Bounds

- Validate sample indexes before access.
- Keep out-of-range failures explicit.
- Avoid exposing backing collections through public raster contracts.

## Samples

- Store domain-specific sampled values behind a common interface.
- Support conversion into raster-friendly data.
