# Convert a Raster to an Image

Convert a numeric or classified raster to a supported Spatial2D color type, then encode it with
`SaveAsPng` or `SaveAsBmp`. Both ordinary `Raster<T>` and geometry-aware `SpatialRaster<T>` can be
exported.

## Map values to image colors

The following example converts normalized elevation samples to an RGBA heat map. `float.NaN` marks
samples outside the finite hex map and becomes a transparent pixel:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var rasterGeometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(3f, 2f),
    resolution: new VectorXYInt(3, 2));

var elevationRaster = new SpatialRaster<float>(
    rasterGeometry,
    new[]
    {
        0f, 0.25f, 0.5f,
        float.NaN, 0.75f, 1f
    });

SpatialRaster<RGBA8BitColor> imageRaster =
    elevationRaster.MapValues(ToColor);

static RGBA8BitColor ToColor(float elevation)
{
    return float.IsNaN(elevation)
        ? new RGBA8BitColor(0, 0, 0, 0)
        : RGBA8BitColor.FromTemperature(elevation);
}
```

`MapValues` calls the selector once for every raster cell and creates a new values array. When the
source is spatial, the result preserves the same `RasterGeometry`; when the source is a regular
`Raster<T>`, the result is another regular raster with the same resolution.

`FromTemperature` expects a normalized value and clamps finite values outside the `0` to `1` range.
Handle sentinels such as `NaN` explicitly before passing them to a palette. Use an RGBA type when
outside-map samples must be transparent; grayscale types do not contain an alpha channel.

If `HexMap.Rasterize` already returns `Gray8BitColor`, `Gray16BitColor`, `RGBA8BitColor`, or
`RGBA16BitColor`, skip `MapValues` and export that raster directly.

## Save a PNG file

Add the imaging namespace, create the destination directory, and call `SaveAsPng`:

```csharp
using System.IO;

string outputDirectory = Path.GetFullPath("output");
Directory.CreateDirectory(outputDirectory);

string outputPath = Path.Combine(outputDirectory, "hex-map.png");
imageRaster.SaveAsPng(outputPath);

Console.WriteLine(
    $"Saved {imageRaster.Resolution.X} x {imageRaster.Resolution.Y} pixels to {outputPath}");
```

The path overload creates or overwrites the file, but its parent directory must already exist.
Image encoding uses only the raster resolution and cell values. A `SpatialRaster<T>` keeps its
world-space `Geometry` in memory, but PNG and BMP files do not store that geometry.

The encoder handles the raster's row orientation when writing image scanlines; do not reverse the
rows manually.

## Write to a stream

Use the stream overload when returning an image from a web endpoint or adding it to an archive:

```csharp
using var output = new MemoryStream();
imageRaster.SaveAsPng(output);

byte[] pngBytes = output.ToArray();
```

The caller owns the stream and decides when to dispose it. It must be writable. The same overload
is available for all supported PNG color types.

## Choose a format and precision

| Raster cell type | PNG | BMP | Typical use |
|---|---:|---:|---|
| `Gray8BitColor` | Yes | Yes | Masks and compact grayscale previews |
| `Gray16BitColor` | Yes | No | Height, distance, or other precise scalar data |
| `RGBA8BitColor` | Yes | Yes | Ordinary color images with transparency |
| `RGBA16BitColor` | Yes | No | High-precision gradients and composition |

Call `SaveAsBmp` instead of `SaveAsPng` when an uncompressed 8-bit BMP is required. A generic
numeric raster such as `Raster<float>` cannot be exported directly: first define how each value
maps to one of the supported color types.

See [Rasterize HexMap Values](rasterize-hex-map-values.md) for creating the source raster and
[Create a Barycentric Raster](create-a-barycentric-raster.md) for smooth interpolation between hex
centers. The general raster model is described in
[Rasterization](../../concepts/rasterization.md).
