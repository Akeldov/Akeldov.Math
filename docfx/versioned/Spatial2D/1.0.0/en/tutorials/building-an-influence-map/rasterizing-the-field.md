# Rasterizing the Field

The field is continuous: it can be evaluated at any finite point. To produce an image, define a
rectangular world area and the resolution of its discrete grid.

Add these namespaces:

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
```

Then create the raster geometry and heatmap:

```csharp
var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(100f, 70f),
    resolution: new VectorXYInt(800, 560));

SpatialRaster<RGBA16BitColor> raster = field.RasterizeHeatMap(grid);
```

<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> separates world geometry from image
resolution:

- `origin` is the lower corner of the area in world coordinates;
- `size` is the width and height in world units;
- `resolution` is the number of raster columns and rows.

The world and image have matching aspect ratios in this example, so neither axis is distorted.
Each raster element receives the field value sampled at the center of its cell.

`RasterizeHeatMap` maps the `field.Min`–`field.Max` range to 16-bit RGBA colors. This is convenient
for inspecting and visualizing a field. Use a custom rasterizer if the application needs another
palette or a numeric raster.

The raster is now in memory. Save it on the final step: [Exporting the Image](exporting-the-image.md).
