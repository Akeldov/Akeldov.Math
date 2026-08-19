# Mapping Values to Colors

Convert the interpolated scalar raster to image pixels. The built-in temperature palette maps low
values to blue and high values to red, while samples outside the map become transparent.

## Create an RGBA raster

Add this code after creating `elevationRaster`:

```csharp
SpatialRaster<RGBA8BitColor> colorRaster =
    elevationRaster.MapValues(ToColor);

static RGBA8BitColor ToColor(float elevation)
{
    return float.IsNaN(elevation)
        ? new RGBA8BitColor(0, 0, 0, 0)
        : RGBA8BitColor.FromTemperature(elevation);
}
```

`MapValues` invokes `ToColor` once per pixel and creates a new raster with the same
`RasterGeometry`. `FromTemperature` expects a finite normalized value and clamps values outside
the `0` to `1` interval. Checking the `NaN` sentinel first avoids passing an invalid value and
preserves the transparent margin.

You can replace `ToColor` without rebuilding either lookup raster. For example, a grayscale
preview can return `Gray8BitColor`, while a classified map can select discrete colors instead of
using a continuous palette.

Continue with [Exporting the Image](exporting-the-image.md).
