# Non-Spatial Rasters

A non-spatial raster stores a rectangular grid of values with a resolution, but no world-space
origin or size. Use it for masks, counters, scalar buffers, and images addressed by cell indices.

`Raster<T>` is the general container; the Boolean and numeric specializations add cell-wise
operators. Their constructors retain the supplied array, and their indexers and `Values` are mutable.
The read-only access contract is <xref:Akeldov.Math.Spatial2D.Rasterization.IRaster`1>; it does not
make the underlying buffer immutable.

## Storage and operations

Values use row-major order: `(x, y)` corresponds to `y * Resolution.X + x`. Resolution components
must be positive and the array must have exactly one element per cell. Access cells through
`raster[x, y]`, `raster[index]`, or a `VectorXYInt` index.

`MapValues` creates a new `Raster<TResult>`. Specialized operators also allocate new mutable
results owned by the caller; binary raster operands must have matching resolutions. They do not
resize or resample inputs. See [Spatial Rasters](spatial-rasters.md) when cells need world bounds,
and [Color Rasters](color-rasters.md) for image values.

## Raster types

- [`Raster<T>`](non-spatial-rasters/raster.md)
- [`BoolRaster`](non-spatial-rasters/bool-raster.md)
- [`IntRaster`](non-spatial-rasters/int-raster.md)
- [`FloatRaster`](non-spatial-rasters/float-raster.md)
