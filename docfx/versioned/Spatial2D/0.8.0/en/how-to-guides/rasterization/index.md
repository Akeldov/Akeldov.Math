# Rasterization

Rasterization samples continuous geometry or fields on a rectangular grid. Use it to create image
data, masks, heatmaps, signed-distance fields, and other regular spatial buffers.

Spatial2D keeps world geometry separate from image resolution: a
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> defines the sampled bounds and cell
count, while a rasterizer or selector converts each sampled value into the desired cell type.

## Choose the Workflow

| Goal | Approach |
| --- | --- |
| Draw a curve as a stroke | Rasterize its unsigned distance with a width, fade distance, and foreground/background colors. |
| Create an inside/outside mask | Rasterize a contour or region and map signed distances at or below zero to the inside value. |
| Preserve distances for later processing | Rasterize into `SpatialRaster<float>` or map distances to 16-bit grayscale. |
| Visualize an influence field | Sample the field over a grid and convert its value range to colors. |
| Compose several geometry objects | Add distance-based layers to `GeometryScene<TColor>`. |
| Export an image | Use a supported grayscale or RGBA cell type, then call `SaveAsPng` or `SaveAsBmp`. |

## Core Types

| Type | Purpose |
| --- | --- |
| <xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> | Connects raster cells to rectangular world-space bounds. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.Raster`1> | Stores a mutable non-spatial rectangular value grid. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.SpatialRaster`1> | Stores mutable values together with their world geometry. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.IRasterizer`2> | Produces a raster from a source and resolution. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRasterizer`2> | Produces a spatial raster from a source and `RasterGeometry`. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.GeometryScene`1> | Composes several rasterized geometry layers. |

## Basic Pattern

Define the world bounds and resolution, rasterize a source, then export the returned raster:

```csharp
var grid = new RasterGeometry(
    origin: new PointXY(-1f, -1f),
    size: new VectorXY(6f, 4f),
    resolution: new VectorXYInt(600, 400));

SpatialRaster<Gray8BitColor> mask = region.Rasterize(
    signedDistance => signedDistance <= 0f
        ? Gray8BitColor.White
        : Gray8BitColor.Black,
    grid);

mask.SaveAsPng("region-mask.png");
```

Cells are sampled at their centers. `Origin` is the lower-left world corner, `Size` is measured in
world units, and `Resolution` controls the number of columns and rows independently.

## Pick the Output Precision

- `Gray8BitColor` is compact and suitable for masks.
- `Gray16BitColor` preserves more scalar or gradient precision.
- `RGBA8BitColor` is suitable for ordinary color output with alpha.
- `RGBA16BitColor` is useful for high-precision gradients and composition.

Use a `SpatialRaster<T>` while later operations still need to map cells back to world coordinates.
Rasterization returns a new mutable raster owned by the caller.

For a complete workflow covering a region with a hole, signed-distance mapping, 16-bit output,
and PNG export, see [Rasterize a Signed-Distance Field](rasterize-a-signed-distance-field.md).
For the broader model, see [Rasterization Concepts](../../concepts/rasterization.md).
