# Spatial Rasters

A spatial raster associates a rectangular value grid with <xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry>:
world-space `Origin`, `Size`, and cell `Resolution`. `CellSize` and `GetCellCenter(x, y)` connect
cell indices to world coordinates. Use it for sampled fields, distance maps, spatial masks, and drawings.

The common contract is <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRaster`1>, which extends
`IRaster<T>` with `Geometry`. `SpatialRaster<T>` is the general container. The three specialized
types provide operators while retaining the same geometry and mutable row-major storage.

## Geometry and ownership

Constructors retain the supplied value array. Operations between specialized spatial rasters
require equal origin, size, and resolution. Mixed operators with non-spatial rasters require
equal resolution and preserve the spatial operand's geometry; there is no automatic resampling.

`MapValues` called through `ISpatialRaster<T>` preserves geometry and creates a new mutable
`SpatialRaster<TResult>` owned by the caller. Keep the spatial type visible when selecting an
overload: treating a raster only as `IRaster<T>` selects operations that return a non-spatial result.

Color is a separate choice: `SpatialRaster<RGBA8BitColor>` is both spatial and a
[Color Raster](color-rasters.md).

## Raster types

- [`SpatialRaster<T>`](spatial-rasters/spatial-raster.md)
- [`SpatialBoolRaster`](spatial-rasters/spatial-bool-raster.md)
- [`SpatialIntRaster`](spatial-rasters/spatial-int-raster.md)
- [`SpatialFloatRaster`](spatial-rasters/spatial-float-raster.md)
