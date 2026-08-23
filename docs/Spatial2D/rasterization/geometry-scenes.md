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

var grid = new RasterGeometry(
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

var background = RGBA16BitColor.FromNormalized(1f, 1f, 1f, 1f);
SpatialRaster<RGBA16BitColor> raster = new GeometryScene<RGBA16BitColor>(background, RGBA16BitColor.AlphaOver)
    .AddSignedPointDistanceBasedLayer(region, fill, edgeFalloff: 0.5f)
    .AddPointDistanceBasedLayer(segment, stroke, fillDistance: 1.5f, edgeFalloff: 0.5f)
    .AddPointDistanceBasedLayer(points, marker, fillDistance: 2f, edgeFalloff: 0.5f)
    .Rasterize(grid);

raster.SaveAsPng("scene.png");
```

## Snapshot Examples

The examples below use the same scene ideas and approved outputs as the
`GeometrySceneSnapshotTests` snapshot tests.

### Smiley Scene

Use signed distance layers for filled closed shapes, and unsigned distance
layers for strokes, points, and open curves. Layers are sampled in insertion
order and blended with the scene's blend function.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(100f, 70f),
    resolution: new VectorXYInt(180, 126));

var face = new Circle(new PointXY(50f, 35f), 24f);
var smile = new Arc(
    new PointXY(50f, 36f),
    radius: 15f,
    startAngle: 7f * MathF.PI / 6f,
    endAngle: 11f * MathF.PI / 6f);

var eyes = new[]
{
    new PointXY(40f, 43f),
    new PointXY(60f, 43f)
};
var highlights = new[]
{
    new PointXY(39f, 43.9f),
    new PointXY(59f, 43.9f)
};
var cheeks = new[]
{
    new PointXY(34f, 33f),
    new PointXY(66f, 33f)
};

RGBA16BitColor background = RGBA16BitColor.FromNormalized(0.965f, 0.972f, 0.982f, 1f);
RGBA16BitColor faceFill = RGBA16BitColor.FromNormalized(1.000f, 0.810f, 0.145f, 0.66f);
RGBA16BitColor faceBoundary = RGBA16BitColor.FromNormalized(0.055f, 0.085f, 0.165f, 0.95f);
RGBA16BitColor eyeColor = RGBA16BitColor.FromNormalized(0.050f, 0.060f, 0.080f, 1f);
RGBA16BitColor highlightColor = RGBA16BitColor.FromNormalized(1f, 1f, 1f, 0.75f);
RGBA16BitColor cheekColor = RGBA16BitColor.FromNormalized(0.940f, 0.260f, 0.310f, 0.55f);

SpatialRaster<RGBA16BitColor> raster = new GeometryScene<RGBA16BitColor>(
        background,
        RGBA16BitColor.AlphaOver)
    .AddSignedPointDistanceBasedLayer(face, faceFill, edgeFalloff: 0.55f)
    .AddPointDistanceBasedLayer(face, faceBoundary, fillDistance: 1.45f, edgeFalloff: 0.45f)
    .AddPointDistanceBasedLayer(eyes, eyeColor, fillDistance: 2.4f, edgeFalloff: 0.45f)
    .AddPointDistanceBasedLayer(highlights, highlightColor, fillDistance: 0.65f, edgeFalloff: 0.22f)
    .AddPointDistanceBasedLayer(cheeks, cheekColor, fillDistance: 3.2f, edgeFalloff: 1.1f)
    .AddPointDistanceBasedLayer(smile, faceBoundary, fillDistance: 2.2f, edgeFalloff: 0.55f)
    .Rasterize(grid);

raster.SaveAsPng("smiley.png");
```

![Smiley made from a filled circle, point eyes, and an arc smile](../../assets/spatial2d/geometry-scenes/geometry-scene-smiley-circle-arc-rgba16.png)

`startAngle` and `endAngle` are in radians. The arc and point layers use
`fillDistance` as their solid stroke radius, then fade across `edgeFalloff`.

### Prism Scene

When a layer needs to vary along a curve, use a parameterized projection layer.
The callback receives the sampled point and a `ParameterizedCurveProjection`,
so it can react to both distance from the curve and the curve coordinate.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var prism = new CompositeContour(new IContourPath[]
{
    new ParameterizedSegment(new PointXY(34f, 18f), new PointXY(52f, 49.177f)),
    new ParameterizedSegment(new PointXY(52f, 49.177f), new PointXY(70f, 18f)),
    new ParameterizedSegment(new PointXY(70f, 18f), new PointXY(34f, 18f))
});

var outgoingBeam = new ParameterizedSegment(
    new PointXY(60.762f, 34f),
    new PointXY(126.036f, 10f))
    .ExtendStart(2f);

RGBA16BitColor[] rainbowStops =
{
    RGBA16BitColor.FromNormalized(1.000f, 0.090f, 0.080f, 0.78f),
    RGBA16BitColor.FromNormalized(1.000f, 0.520f, 0.050f, 0.78f),
    RGBA16BitColor.FromNormalized(1.000f, 0.930f, 0.120f, 0.78f),
    RGBA16BitColor.FromNormalized(0.160f, 0.860f, 0.280f, 0.78f),
    RGBA16BitColor.FromNormalized(0.100f, 0.560f, 1.000f, 0.78f),
    RGBA16BitColor.FromNormalized(0.520f, 0.220f, 1.000f, 0.78f)
};

RGBA16BitColor ToRainbowColor(PointXY point, ParameterizedCurveProjection projection)
{
    if (prism.Encloses(point))
        return RGBA16BitColor.Transparent;

    float beamHalfWidth = 0.8f + (projection.CurveCoordinate + 3f) / 9f;
    float signedDistance = outgoingBeam.GetHalfPlaneSide(point) switch
    {
        HalfPlaneSide.Left => projection.Distance,
        HalfPlaneSide.Right => -projection.Distance,
        _ => 0f
    };

    float spectralPosition = System.Math.Clamp(
        (beamHalfWidth - signedDistance) / (2f * beamHalfWidth),
        0f,
        1f);
    float scaledPosition = spectralPosition * (rainbowStops.Length - 1);
    int stopIndex = (int)MathF.Floor(scaledPosition);

    RGBA16BitColor rainbowColor = stopIndex >= rainbowStops.Length - 1
        ? rainbowStops[rainbowStops.Length - 1]
        : RGBA16BitColor.Blend(
            rainbowStops[stopIndex],
            rainbowStops[stopIndex + 1],
            scaledPosition - stopIndex);

    if (projection.Distance <= beamHalfWidth)
        return rainbowColor;

    float edgeFalloff = 0.48f;
    float edgeDistance = projection.Distance - beamHalfWidth;
    float edgeCoverage = 1f - MathF.Min(edgeDistance, edgeFalloff) / edgeFalloff;
    return rainbowColor.ScaleAlpha(edgeCoverage);
}

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(100f, 70f),
    resolution: new VectorXYInt(720, 504));

RGBA16BitColor background = RGBA16BitColor.FromNormalized(0.018f, 0.020f, 0.030f, 1f);
RGBA16BitColor prismFill = RGBA16BitColor.FromNormalized(0.500f, 0.640f, 0.790f, 1f);

SpatialRaster<RGBA16BitColor> raster = new GeometryScene<RGBA16BitColor>(
        background,
        RGBA16BitColor.AlphaOver)
    .AddParameterizedProjectionBasedLayer(outgoingBeam, ToRainbowColor)
    .AddSignedPointDistanceBasedLayer(prism, distance =>
        distance <= 0f
            ? prismFill.ScaleAlpha(1f / (1f + -distance * 2f))
            : prismFill.ScaleAlpha(1f / (1f + distance * 5f)))
    .Rasterize(grid);

raster.SaveAsPng("prism.png");
```

![Triangular prism scene with an incoming beam and rainbow output](../../assets/spatial2d/geometry-scenes/geometry-scene-triangular-prism-rgba16.png)

The snapshot test adds incoming and in-prism beam layers with the same API. Use
`AddParameterizedProjectionBasedLayer` when the color or width should depend on
where a pixel projects onto a path.

For custom styling, use distance-mapping layers directly:

```csharp
GeometryScene<RGBA16BitColor> scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
    .AddPointDistanceBasedLayer(segment, distance =>
        distance <= 1f
            ? RGBA16BitColor.FromNormalized(1f, 0f, 0f, 1f)
            : default)
    .AddSignedPointDistanceBasedLayer(region, signedDistance =>
        signedDistance <= 0f
            ? RGBA16BitColor.FromNormalized(0f, 0f, 1f, 0.2f)
            : default);
```

For non-RGBA output, pass the default color operations explicitly:

```csharp
SpatialRaster<int> labels = new GeometryScene<int>(
        backgroundColor: 0,
        defaultLayerBlend: (current, next) => next == 0 ? current : next)
    .AddSignedPointDistanceBasedLayer(region, signedDistance => signedDistance <= 0f ? 1 : 0)
    .AddPointDistanceBasedLayer(segment, distance => distance <= 1.5f ? 2 : 0)
    .Rasterize(grid);
```
