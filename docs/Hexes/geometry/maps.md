# Geometry Maps

Geometry maps sample Spatial2D values from a hex field.

## `HexCenterMap`

- Maps each hex index to its world-space center.
- Implements the common hex map contract.
- Preserves layout and geometric parameters.

```csharp
var map = new HexCenterMap(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    apothem: 8f,
    layout: Layout.OddR);

RasterGeometry grid = HexFieldGeometryRGBA16BitRasterizer.CreateGrid(
    map,
    pixelsPerApothem: 24f);

SpatialRaster<RGBA16BitColor> raster = new HexFieldGeometryRGBA16BitRasterizer(ToColor)
    .Rasterize(map, grid);

raster.SaveAsPng("hex-center-map-odd-r-rgba16.png");

static RGBA16BitColor ToColor(PointXY center)
{
    return RGBA16BitColor.FromNormalized(
        red: 0.22f + 0.04f * center.X,
        green: 0.18f + 0.05f * center.Y,
        blue: 0.72f - 0.006f * (center.X + center.Y),
        alpha: 1f);
}
```

![HexCenterMap rasterized with center-derived colors](../../assets/hexes/maps/hex-center-map-odd-r-rgba16.png)
