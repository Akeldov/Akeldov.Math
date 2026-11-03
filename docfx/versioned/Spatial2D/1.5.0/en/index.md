# Akeldov.Math.Spatial2D

Spatial2D provides two-dimensional geometry, curves, contours, regions, rasterization,
spatial sampling, partitioning, and influence fields.

## Install

```powershell
dotnet add package Akeldov.Math.Spatial2D --version 1.5.0
```

## New in 1.5.0

- Draw directly on spatial RGBA rasters: curves, contours, filled circles and rectangles,
  arrows with solid or dashed shafts, and coordinate grids with labels.
- Render text using cached TrueType glyphs or the built-in 5x7 pixel font with uppercase and
  lowercase Latin and Russian letters, including Ё/ё.
- Darken and lighten colors using sRGB, linear RGB, or OKLCH.
- Curve drawing skips unaffected bounds and tiles while preserving the existing coverage model.

The release retains the public API of 1.4.0 and targets .NET Standard 2.1 and .NET 6.

[Draw shapes and text on a raster](how-to-guides/rasterization/draw-on-a-raster.md) introduces
the drawing API. See also [numeric and Boolean rasters](how-to-guides/rasterization/work-with-specialized-rasters.md),
[concepts](concepts/index.md), and [tutorials](tutorials/index.md).

[View the package on NuGet](https://www.nuget.org/packages/Akeldov.Math.Spatial2D/)
