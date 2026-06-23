# GeometryScene

`GeometryScene<TColor>` composes points, curves, contours, and regions into one
color buffer. For 16-bit RGBA output, use `GeometryScene<RGBA16BitColor>`.

Layers are sampled in insertion order and composited with each layer's blend function.
Layers created through scene extension methods use the scene's default blend function.
Use unsigned distance layers for points and open curves, and signed distance layers for
contours and regions.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

var grid = new RasterGrid(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(64f, 64f),
    resolution: new VectorXYInt(256, 256));

var region = new Disk(new PointXY(32f, 32f), 18f);
var segment = new Segment(new PointXY(8f, 12f), new PointXY(56f, 52f));
var points = new[]
{
    new PointXY(28f, 30f),
    new PointXY(36f, 34f)
};

RGBA16BitColor fill = RGBA16BitColor.FromNormalized(0.1f, 0.35f, 0.95f, 0.25f);
RGBA16BitColor stroke = RGBA16BitColor.FromNormalized(0.95f, 0.1f, 0.15f, 1f);
RGBA16BitColor marker = RGBA16BitColor.FromNormalized(0.02f, 0.02f, 0.02f, 1f);

RGBA16BitRaster raster = GeometryScenes.CreateRGBA16Bit()
    .Fill(region, fill, edgeFalloff: 0.5f)
    .Stroke(segment, stroke, width: 1.5f, edgeFalloff: 0.5f)
    .Point(points, marker, radius: 2f, edgeFalloff: 0.5f)
    .Rasterize(grid);

raster.SaveAsPng("scene.png");
```

For custom styling, use distance-mapping layers directly:

```csharp
GeometryScene<RGBA16BitColor> scene = GeometryScenes.CreateRGBA16Bit()
    .Distance(segment, distance =>
        distance <= 1f
            ? RGBA16BitColor.FromNormalized(1f, 0f, 0f, 1f)
            : default)
    .SignedDistance(region, signedDistance =>
        signedDistance <= 0f
            ? RGBA16BitColor.FromNormalized(0f, 0f, 1f, 0.2f)
            : default);
```

For non-RGBA output, pass the default color operations explicitly:

```csharp
var labels = new GeometryScene<int>(
        backgroundColor: 0,
        blend: (current, next) => next == 0 ? current : next,
        applyCoverage: (color, coverage) => coverage > 0f ? color : 0)
    .Fill(region, color: 1)
    .Stroke(segment, color: 2, width: 1.5f)
    .RasterizeValues(grid);
```
