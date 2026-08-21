# Rasterize HexMap Values

Use `Rasterize` to sample one value from a hex map at the center of every cell in a rectangular
raster. A selector converts each source value into the result type, which may be an image color,
a number, or another value used by later processing.

## Rasterize a spatial map

Create a <xref:Akeldov.Math.Hexes.SpatialHexMap`1> when the map's radius and world-space placement
must be preserved. The following example maps three terrain classes to RGBA colors:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

var terrain = new SpatialHexMap<int>(geometry, new[]
{
    0, 0, 1, 1,
    0, 1, 1, 2,
    1, 1, 2, 2
});

SpatialRaster<RGBA8BitColor> raster = terrain.Rasterize(
    pixelsPerApothem: 24f,
    margin: geometry.Radius,
    colorSelector: TerrainColor);

static RGBA8BitColor TerrainColor(int terrainType)
{
    return terrainType switch
    {
        0 => new RGBA8BitColor(40, 104, 216, byte.MaxValue),
        1 => new RGBA8BitColor(50, 160, 72, byte.MaxValue),
        2 => new RGBA8BitColor(136, 136, 136, byte.MaxValue),
        _ => new RGBA8BitColor(224, 48, 48, byte.MaxValue)
    };
}
```

`pixelsPerApothem` controls the sampling density. `margin` adds the specified number of world units
on every side of the map's bounding box. Here it is one hex radius, so the output includes cells
outside the finite topology.

For each output cell, `Rasterize` takes its center point, finds the containing hex, reads that hex's
value, and calls `colorSelector`. Cells whose centers map outside the source topology retain
`default(RGBA8BitColor)`, whose alpha channel is zero, so the outer margin is transparent. The
selector is not called for those cells.

## Reuse an exact sampling grid

Pass an explicit `RasterGeometry` when this result must align cell for cell with another spatial
raster:

```csharp
RasterGeometry rasterGeometry = terrain.ToRasterGeometry(
    pixelsPerApothem: 24f,
    margin: geometry.Radius);

SpatialRaster<RGBA8BitColor> raster = terrain.Rasterize(
    rasterGeometry,
    TerrainColor);
```

The result retains the exact supplied geometry, including its origin, world-space size, and
resolution. A custom grid may cover the whole map, crop it, or extend beyond it. Reuse the same
instance for every layer that must share pixel coordinates.

## Rasterize a logical map

An <xref:Akeldov.Math.Hexes.IHexMap`1> contains topology and values but no world-space geometry. Give
its overload the required pixel resolution directly:

```csharp
var values = new HexMap<int>(
    new HexMapTopology(2, 2, Layout.OddR),
    new[]
    {
        10, 20,
        30, 40
    });

Raster<byte> preview = values.Rasterize(
    resolution: new VectorXYInt(320, 240),
    colorSelector: value => (byte)(value * 5));
```

This form temporarily places the topology at the default origin with unit radius and fits it into
the requested resolution. It returns `Raster<T>` rather than `SpatialRaster<T>`, so the result has
no world placement. Use it for logical previews; use a spatial map when origin, radius, or cell size
matters.

## Choose between direct sampling and interpolation

Direct map rasterization assigns exactly one containing hex to each raster cell. It does not blend
neighbors or calculate the area covered by a pixel, so values change discretely across hex edges.

If a sample must blend values from three nearby hex centers, combine an index raster with a
barycentric raster instead. See [Create a Barycentric Raster](create-a-barycentric-raster.md). To
write a color raster to a file, continue with
[Convert a Raster to an Image](convert-a-raster-to-an-image.md). The complete sampling model is
described in [Rasterization](../../concepts/rasterization.md).
