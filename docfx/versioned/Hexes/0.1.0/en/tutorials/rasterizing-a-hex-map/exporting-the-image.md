# Exporting the Image

The color raster is ready to be encoded as a PNG with the Spatial2D imaging extensions.

## Save and run

Add the final statements after constructing `colorRaster`:

```csharp
string outputPath = Path.GetFullPath("hex-elevation.png");
colorRaster.SaveAsPng(outputPath);

Console.WriteLine(
    $"Raster: {colorRaster.Resolution.X} x {colorRaster.Resolution.Y}");
Console.WriteLine($"Saved: {outputPath}");
```

Run the project:

```powershell
dotnet run
```

The exact resolution is derived from the map bounds, margin, and requested pixel density. The
program prints that resolution and the absolute path to `hex-elevation.png`.

![Smoothly interpolated hex-map elevation values](../../../../../../assets/hexes/tutorials/rasterizing-a-hex-map.png)

The image shows the procedural elevation field blended between hex centers. The half-radius
margin is transparent, and finite-map boundary samples are normalized from whichever neighboring
cells remain present.

You now have a reusable two-stage pipeline: index and barycentric rasters encode spatial
relationships, while the elevation and color rasters can be regenerated whenever map values or
the palette change. See [Rasters](../../concepts/data-storage/rasters.md) for the complete raster
families and [Rasterization](../../concepts/rasterization.md) for direct, non-interpolated map
rendering.
