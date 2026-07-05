# `IGrid<TValue>`

`IGrid<TValue>` is the common contract for sampled grid values.

## Metadata

- Width.
- Height.

## Access

- Index-based sampled value access.
- Flat sampled value indexing.
- Implementations decide which domain their samples represent.

## Role

- Keeps sampled data independent from topology, geometry, or chromatization.
- Allows rasterization helpers to consume grids through a common surface.

## Rasterization

- `IGrid<TValue>` can be rasterized with an explicit `SpatialRasterGrid`.
- The raster grid resolution must match the grid width and height.
- Domain-specific grid types may expose convenience rasterization methods that supply their own raster geometry.
