# Rasterize a Signed-Distance Field

A signed-distance field stores the shortest distance to a boundary together with inside/outside
information. Spatial2D uses negative values inside a region, zero on its boundary, and positive
values in holes and outside the region.

This guide builds a square region with a square hole, maps distances from `-1` to `1` into 16-bit
grayscale, and saves the result as a PNG image.

## Create the Region

Add the required namespaces and define two closed contours:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

IRegion region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});
```

`ContourBasedRegion` applies the even-odd fill rule. The outer contour creates the filled area;
the nested contour toggles that area back to empty and therefore becomes a hole.

Add this helper after the top-level statements:

```csharp
static IContour CreateSquareContour(
    float left,
    float bottom,
    float right,
    float top)
{
    return new CompositeContour(new IContourPath[]
    {
        new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
        new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
        new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
        new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
    });
}
```

The paths are ordered head to tail, and the last path ends at the first path's start. This is the
closed-chain contract required by `CompositeContour`. Each `IContourPath` supports fill-rule
crossings and directly declares the ray-intersection query used by the composite.

## Define the Sampling Grid

Include half a world unit of padding around the outer boundary:

```csharp
var grid = new RasterGeometry(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(320, 320));
```

The grid covers `[-0.5, 4.5]` on both axes. Its square world bounds and square resolution preserve
the geometry's aspect ratio. Each output cell samples the region at the cell center.

## Map Distance to Grayscale

Map `-1` to black, the boundary value `0` to middle gray, and `1` to white. Clamp farther values
to the ends of that interval:

```csharp
var rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(
    signedDistance =>
    {
        float normalized = Math.Clamp(
            (signedDistance + 1f) / 2f,
            0f,
            1f);

        return new Gray16BitColor(
            (ushort)(normalized * ushort.MaxValue));
    });
```

The transfer function controls only visualization. It does not modify the region or its distance
calculation. Change the interval when the useful distance scale in your application is larger or
smaller than one world unit.

## Rasterize and Export

```csharp
SpatialRaster<Gray16BitColor> raster =
    region.Rasterize(grid, rasterizer);

raster.SaveAsPng("signed-distance.png");
```

The returned raster is new, mutable, and owned by the caller. It retains `grid` through its
`Geometry` property, so cells can still be related to world coordinates. PNG export writes the
resolution and grayscale values; it does not embed the world-space bounds.

## Complete Code

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

IRegion region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});

var grid = new RasterGeometry(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(320, 320));

var rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(
    signedDistance =>
    {
        float normalized = Math.Clamp(
            (signedDistance + 1f) / 2f,
            0f,
            1f);

        return new Gray16BitColor(
            (ushort)(normalized * ushort.MaxValue));
    });

SpatialRaster<Gray16BitColor> raster =
    region.Rasterize(grid, rasterizer);

raster.SaveAsPng("signed-distance.png");

static IContour CreateSquareContour(
    float left,
    float bottom,
    float right,
    float top)
{
    return new CompositeContour(new IContourPath[]
    {
        new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
        new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
        new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
        new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
    });
}
```

Use a binary selector such as `distance <= 0f` when you only need a mask. Keep or generate floating-
point distance values when later processing needs thresholds, offsets, collision margins, or a
different transfer function.

For the underlying raster and grid model, see [Rasterization Concepts](../../concepts/rasterization.md).
