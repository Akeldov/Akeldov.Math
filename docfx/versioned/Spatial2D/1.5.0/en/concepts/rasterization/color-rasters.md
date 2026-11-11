# Color Rasters

A color raster stores grayscale or RGBA values in each cell. Color describes the **value type**,
independently of spatial bounds: use `Raster<TColor>` for an image buffer or
`SpatialRaster<TColor>` when pixels must remain tied to world coordinates. These are generic
containers with color arguments, not separate `GrayRaster` or `RGBARaster` classes.

## Choose a format

| Cell type | Channels | Range per channel | PNG | BMP |
|---|---|---|---|---|
| [Gray8BitColor](color-rasters/gray8.md) | Intensity | 0–255 | Yes | Yes |
| [Gray16BitColor](color-rasters/gray16.md) | Intensity | 0–65535 | Yes | No |
| [RGBA8BitColor](color-rasters/rgba8.md) | Red, green, blue, alpha | 0–255 | Yes | Yes |
| [RGBA16BitColor](color-rasters/rgba16.md) | Red, green, blue, alpha | 0–65535 | Yes | No |

Colors are immutable values; replace a raster cell to change a pixel. `FromNormalized` converts
channels from `[0, 1]` with clamping and rounding. `Blend` interpolates stored channels.

## Convert and export

Use `MapValues` to convert a numeric or Boolean raster into colors. Choose the physical value
range explicitly before converting distances or heights; a color image does not preserve that mapping.
`SaveAsPng` and `SaveAsBmp` consume `IRaster<TColor>`, so both container variants are accepted.
`RasterImageLoader` loads supported images into non-spatial `Raster<TColor>` buffers.

Image files store pixels, not `RasterGeometry`. Keep world bounds separately if they are needed
after loading. See [Imaging](../imaging.md) for interpolation, alpha composition, and encoding.

## Raster types

- [`Raster<Gray8BitColor>`](color-rasters/gray8.md)
- [`Raster<Gray16BitColor>`](color-rasters/gray16.md)
- [`Raster<RGBA8BitColor>`](color-rasters/rgba8.md)
- [`Raster<RGBA16BitColor>`](color-rasters/rgba16.md)
