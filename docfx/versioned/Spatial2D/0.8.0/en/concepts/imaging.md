# Imaging

Imaging describes how Spatial2D represents colors, combines them, and exports a color raster as
an image. The types and export extensions live in <xref:Akeldov.Math.Spatial2D.Imaging>.

[Rasterization](rasterization.md) defines where to sample geometry or a field and which value to
store in each cell. Imaging supplies color values for those cells and encodes the resulting grid
as PNG or BMP. The same export methods work with a manually populated raster.

## Colors and precision

The four color types are immutable value types. Grayscale stores one intensity in `Value`; RGBA
stores red, green, blue, and alpha in `R`, `G`, `B`, and `A`.

| Color type | Channel range | Typical use |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Imaging.Gray8BitColor> | `byte`: 0–255 | Masks and compact scalar images. |
| <xref:Akeldov.Math.Spatial2D.Imaging.Gray16BitColor> | `ushort`: 0–65535 | Height maps and distance visualizations that need more precision. |
| <xref:Akeldov.Math.Spatial2D.Imaging.RGBA8BitColor> | `byte`: 0–255 per channel | Ordinary color images with transparency. |
| <xref:Akeldov.Math.Spatial2D.Imaging.RGBA16BitColor> | `ushort`: 0–65535 per channel | Fine gradients and layered composition. |

For grayscale, zero is black and the maximum is white. For alpha, zero is fully transparent and
the maximum is fully opaque. Grayscale colors have no alpha channel: a black mask cell represents
zero intensity, and its interpretation as empty or transparent belongs to the consuming code.

Constructors accept integer channel values. `FromNormalized` accepts floating-point channels in
the range `[0, 1]`, clamps values outside that range, and rounds to the nearest representable
integer channel value:

```csharp
using Akeldov.Math.Spatial2D.Imaging;

Gray8BitColor middleGray = Gray8BitColor.FromNormalized(0.5f); // Value = 128
RGBA16BitColor translucentBlue = RGBA16BitColor.FromNormalized(
    red: 0f, green: 0f, blue: 1f, alpha: 0.5f);
```

An 8-bit channel has 256 possible values; a 16-bit channel has 65536. More bits reduce
quantization when mapping continuous values to colors, but the result still has finite precision.
Use 8-bit colors for compact output and 16-bit colors when the extra precision matters.

Keep physical quantities in a numeric raster while later calculations still need their original
values. Mapping a signed distance or height to grayscale requires choosing a value range;
normalization, clipping, and rounding can discard information. The image does not record how to
recover those source values.

## Interpolation and alpha composition

All four color types provide `Blend(from, to, amount)`. It interpolates each stored channel
independently, including alpha for RGBA. An amount of zero returns the starting color and one
returns the ending color; amounts outside `[0, 1]` are clamped. Use it for gradients and transitions
between colors.

`RGBA16BitColor.AlphaOver(background, foreground)` composites the foreground over the background
using their alpha values. The argument order matters. An opaque foreground covers the background;
a transparent foreground leaves it visible. Compositing over an opaque background produces an
opaque result.

```csharp
RGBA16BitColor background = RGBA16BitColor.FromNormalized(0f, 0f, 1f, 1f);
RGBA16BitColor foreground = RGBA16BitColor.FromNormalized(1f, 0f, 0f, 0.5f);

RGBA16BitColor transition = RGBA16BitColor.Blend(background, foreground, 0.5f);
RGBA16BitColor composite = RGBA16BitColor.AlphaOver(background, foreground);
// transition has alpha approximately 0.75; composite is fully opaque.
```

`AlphaOver` uses straight alpha: RGB channels describe the color independently of alpha. Pass
full RGB values together with the desired opacity. For example, half-transparent red is
`FromNormalized(1f, 0f, 0f, 0.5f)`.

`RGBA16BitColor` also provides `ScaleAlpha(coverage)` to reduce opacity for partial coverage. For
example, `foreground.ScaleAlpha(0.5f)` halves its existing alpha. `AlphaOver` and `ScaleAlpha` are
available on the 16-bit RGBA type; `Blend` is also available on the other color types.

These operations calculate directly with the stored channel values, without automatic color-space
conversion. A [geometry scene](rasterization.md#compose-a-geometry-scene) can use `AlphaOver` as
its blend function to accumulate layers in insertion order.

## Image export

<xref:Akeldov.Math.Spatial2D.Imaging.RasterPngExtensions> and
<xref:Akeldov.Math.Spatial2D.Imaging.RasterBmpExtensions> provide `SaveAsPng` and `SaveAsBmp`
overloads for a file path or a writable stream.

| Raster cell type | PNG output | BMP output |
|---|---|---|
| `Gray8BitColor` | 8-bit grayscale | 8-bit indexed grayscale |
| `Gray16BitColor` | 16-bit grayscale | No overload |
| `RGBA8BitColor` | 8 bits per RGBA channel | 32-bit BGRA |
| `RGBA16BitColor` | 16 bits per RGBA channel | No overload |

PNG preserves the channel depth of the selected color type, including 16-bit channels and RGBA
alpha. Saving a 16-bit color raster as PNG does not first reduce it to 8 bits. BMP export requires
one of the supported 8-bit color types.

Export consumes <xref:Akeldov.Math.Spatial2D.Rasterization.IRaster`1>, so both
`Raster<TColor>` and `SpatialRaster<TColor>` can be saved. The file stores the pixel resolution
and color values. World-space bounds and cell size from `RasterGeometry` remain application
metadata; retain them separately if the image must map back to world coordinates.

A numeric or Boolean raster needs an explicit mapping to a supported color type before export.
For example, a mask can map `false` to `Gray8BitColor.Black` and `true` to `Gray8BitColor.White`
through `MapValues`. The [Rasterization](rasterization.md#rasterize-fields-and-map-values) page
explains how value mapping relates to the grid and its spatial geometry.

For a complete example, follow
[Exporting the Image](../tutorials/building-an-influence-map/exporting-the-image.md). The
[signed-distance rasterization guide](../how-to-guides/rasterization/rasterize-a-signed-distance-field.md)
also shows how to choose a scalar-to-color mapping and save the result.
