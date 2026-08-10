# Rasterization

Rasterization samples continuous two-dimensional geometry or a field on a rectangular grid and
stores one value per cell. It is the bridge between the Spatial2D geometry model and images,
masks, height maps, distance fields, and other regular data.

Rasterization in Spatial2D separates three decisions:

```text
Geometry or field
        |
        v
RasterGeometry: world bounds, resolution, cell centers
        |
        v
Rasterizer or selector: source value -> cell value
        |
        v
SpatialRaster<TValue> -> optional value mapping -> PNG or BMP
```

Types for grids, rasters, and rasterizers live in the
<xref:Akeldov.Math.Spatial2D.Rasterization> namespace. Image colors and export extensions live in
<xref:Akeldov.Math.Spatial2D.Imaging>.

## Define the sampling grid

<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> describes an axis-aligned rectangle in
world coordinates and its resolution in cells:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    origin: new PointXY(-1f, -1f),
    size: new VectorXY(6f, 4f),
    resolution: new VectorXYInt(600, 400));

VectorXY cellSize = grid.CellSize;       // (0.01, 0.01) world units
PointXY firstSample = grid.GetCellCenter(0, 0);
```

`Origin` is the lower-left corner. Cell `(0, 0)` is therefore the lower-left cell, but sources are
sampled at its center, not at the corner. `CellSize` is `Size / Resolution` independently on each
axis.

When pixel density is more useful than an exact resolution, pass `minimumPixelsPerUnit` instead.
The resulting resolution is rounded up independently on each axis:

```csharp
var densityGrid = new RasterGeometry(
    cornerA: new PointXY(5f, 3f),
    cornerB: new PointXY(-1f, -1f),
    minimumPixelsPerUnit: 100);
```

Corner order does not matter. Bounds must be finite and have positive width and height; resolution
and pixel density must also be positive.

## Choose a raster type

The raster value type is generic: a cell can contain a color, number, label, Boolean mask, or an
application-specific value.

| Type | Spatial bounds | Typical use |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Rasterization.IRaster`1> | No | Read-only access to a rectangular value grid. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.Raster`1> | No | Mutable values when only resolution and row/column indices matter. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRaster`1> | Yes | Read-only raster whose cells must remain tied to world coordinates. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.SpatialRaster`1> | Yes | Mutable raster produced by spatial rasterizers and fields. |

Concrete rasters retain the array passed to their constructor and expose it through `Values`.
The array must contain exactly `Resolution.X * Resolution.Y` elements. Values are stored in
row-major order, so `(x, y)` has flat index `y * Resolution.X + x`.

Use a spatial raster while later processing still needs the original bounds or cell centers. A
`SpatialRaster<TValue>` is also a `Raster<TValue>`, so image export and algorithms that only need
an `IRaster<TValue>` can consume it directly.

## Choose a rasterization strategy

Start from the information available on the source:

| Source or goal | Strategy |
|---|---|
| Any <xref:Akeldov.Math.Spatial2D.Fields.IField`1> | Sample the field at every cell center and map its value with a selector. |
| Point, curve, or other unsigned distance provider | Map the non-negative nearest distance to a grayscale value. |
| [Contour](geometry-model/contours.md) or [region](geometry-model/regions.md) | Map signed distance: negative inside, zero on the boundary, positive outside. |
| Parameterized curve | Map both distance and curve coordinate, for example for a gradient along a path. |
| Several colored geometry objects | Compose distance-based layers in a `GeometryScene<TColor>`. |
| Custom source/value pair | Implement <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRasterizer`2>. |

<xref:Akeldov.Math.Spatial2D.Rasterization.IRasterizer`2> produces a non-spatial `Raster<TValue>`
from a resolution. <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRasterizer`2> instead accepts
a `RasterGeometry` and produces a `SpatialRaster<TValue>`.

## Rasterize a curve stroke

Curve helpers convert distance to a stroke with a configurable fade outside its edge:

```csharp
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var segment = new Segment(
    new PointXY(0f, 0f),
    new PointXY(4f, 2f));

SpatialRaster<Gray8BitColor> stroke = segment.Rasterize(
    curveWidth: 0.12f,
    fadeDistance: 0.04f,
    curveColor: Gray8BitColor.White,
    backgroundColor: Gray8BitColor.Black,
    rasterGeometry: grid);

stroke.SaveAsPng("segment.png");
```

`curveWidth` is the full stroke width in world units. `fadeDistance` is the non-negative distance
outside the stroke edge over which the result blends toward the background. For a collection of
curves, each cell uses the nearest curve distance.

The grid samples once at each cell center. Increasing resolution captures smaller features;
adding a fade band produces smoother-looking edges but does not change the source geometry.

## Build a signed-distance mask

Signed distance preserves inside/outside information and can be mapped to a binary mask, a soft
coverage value, or a distance visualization:

```csharp
using Akeldov.Math.Spatial2D.Regions;

var disk = new Disk(new PointXY(2f, 1f), radius: 0.75f);

SpatialRaster<Gray8BitColor> mask = disk.Rasterize(
    signedDistance => signedDistance <= 0f
        ? Gray8BitColor.White
        : Gray8BitColor.Black,
    grid);

mask.SaveAsPng("disk-mask.png");
```

For a collection, the built-in signed-distance rasterizer uses the minimum signed distance. The
union is inside when at least one source has a negative distance. Keep the raw distance in a
`SpatialRaster<float>` when later processing needs offsets, collision thresholds, or custom
transfer functions rather than an encoded image.

See [Rasterize a signed distance field](../how-to-guides/rasterization/rasterize-a-signed-distance-field.md)
for a focused workflow.

## Rasterize fields and map values

Any [field](fields.md) can be sampled on a grid. The selector converts the sampled domain value to
the desired cell type:

```csharp
SpatialRaster<Gray8BitColor> image = field.Rasterize(
    grid,
    value => Gray8BitColor.FromNormalized(
        (value - field.Min) / (field.Max - field.Min)));
```

Field rasterization visits cell centers in row-major order and returns a new mutable raster owned
by the caller. A zero-width value range needs an application-specific mapping instead of the
normalization shown above.

`MapValues` transforms an existing raster without changing its resolution. When called on an
`ISpatialRaster<TValue>`, it also preserves `Geometry` and returns a new `SpatialRaster<TResult>`:

```csharp
SpatialRaster<bool> occupied = mask.MapValues(color => color.Value != 0);
```

The mapped value array is new, mutable, and owned by the caller.

## Compose a geometry scene

<xref:Akeldov.Math.Spatial2D.Rasterization.GeometryScene`1> combines multiple sources into one
buffer. Layers are sampled in insertion order. Each layer has a blend function; helpers created
through the scene use its default blend function.

```csharp
using Akeldov.Math.Spatial2D.Imaging;

RGBA16BitColor background = RGBA16BitColor.FromNormalized(1f, 1f, 1f, 1f);
RGBA16BitColor fill = RGBA16BitColor.FromNormalized(0.1f, 0.4f, 0.9f, 0.35f);
RGBA16BitColor outline = RGBA16BitColor.FromNormalized(0.05f, 0.08f, 0.15f, 1f);

SpatialRaster<RGBA16BitColor> sceneRaster =
    new GeometryScene<RGBA16BitColor>(background, RGBA16BitColor.AlphaOver)
        .AddSignedPointDistanceBasedLayer(disk, fill, edgeFalloff: 0.02f)
        .AddPointDistanceBasedLayer(segment, outline, fillDistance: 0.06f, edgeFalloff: 0.02f)
        .Rasterize(grid);

sceneRaster.SaveAsPng("scene.png");
```

Use unsigned distance layers for points and open curves. Use signed distance layers for closed
contours and regions that have inside/outside semantics. Parameterized projection layers are
appropriate when color or width must vary along a curve. A scene can also use non-color cell
types when supplied with a suitable background and blending function.

## Choose colors and export

Spatial2D provides four image cell types:

| Cell type | Channels | Suitable for |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Imaging.Gray8BitColor> | 8-bit grayscale | Masks and compact scalar images. |
| <xref:Akeldov.Math.Spatial2D.Imaging.Gray16BitColor> | 16-bit grayscale | Height or distance values that need more precision. |
| <xref:Akeldov.Math.Spatial2D.Imaging.RGBA8BitColor> | 8 bits per RGBA channel | Ordinary color images with alpha. |
| <xref:Akeldov.Math.Spatial2D.Imaging.RGBA16BitColor> | 16 bits per RGBA channel | High-precision composition and gradients. |

`SaveAsPng` supports all four color types and writes to a path or stream. `SaveAsBmp` supports the
8-bit grayscale and 8-bit RGBA types. Export consumes `IRaster<TColor>`: it writes resolution and
cell values, while world-space `Geometry` remains application metadata.

## Practical rules

- Define bounds in world units first, then choose resolution from the smallest feature that must
  remain visible.
- Treat `RasterGeometry` as part of the data contract when raster values must map back to space.
- Use signed distance only for sources with meaningful inside/outside semantics.
- Prefer 16-bit values while composing or preserving scalar precision; reduce to 8-bit only when
  the output format or memory budget calls for it.
- Remember that constructors retain their value arrays. Copy an array first when later mutations
  by the original owner must not affect the raster.

Continue with the [fields](fields.md), [curves](geometry-model/curves.md),
[contours](geometry-model/contours.md), and [regions](geometry-model/regions.md) concepts, or follow
the [signed-distance rasterization guide](../how-to-guides/rasterization/rasterize-a-signed-distance-field.md).
