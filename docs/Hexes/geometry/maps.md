# Geometry Maps

Geometry maps sample Spatial2D values from a hex field.

## `HexCenterMap`

- Maps each hex index to its world-space center.
- Implements the common hex map contract.
- Preserves layout and geometric parameters.

```csharp
var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 8f,
    layout: Layout.OddR);
var map = new HexCenterMap(geometry);

SpatialRaster<RGBA16BitColor> raster = map.Rasterize(
    pixelsPerApothem: 24f,
    margin: 0f,
    colorSelector: ToColor);

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
