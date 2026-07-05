# Geometry Grids

Geometry grids sample Spatial2D values from a hex field.

## `BarycentricTripletGrid`

- Samples barycentric weights for vertex triplets.
- Supports geometry rasterization workflows.
- Exposes sampled grid metadata.

```csharp
var grid = new BarycentricTripletGrid(
    hexWidth: 5,
    hexHeight: 4,
    layout: Layout.OddR,
    hexOrigin: VectorXY.Zero,
    resolution: new VectorXYInt(192, 192));

SpatialRaster<RGBA16BitColor> raster = grid.ToRGBA16BitRaster(ToColor);

raster.SaveAsPng("barycentric-triplet-grid-main-odd-r-rgba16.png");

static RGBA16BitColor ToColor(Triplet<float> barycentric)
{
    ushort main = ToChannel(barycentric.Main);
    return new RGBA16BitColor(main, main, main, ushort.MaxValue);
}

static ushort ToChannel(float value)
{
    value = MathF.Min(MathF.Max(value, 0f), 1f);
    return (ushort)MathF.Round(value * ushort.MaxValue);
}
```

![BarycentricTripletGrid rasterized with the main barycentric weight](../../assets/hexes/grids/barycentric-triplet-grid-main-odd-r-rgba16.png)

## `BarycentricPartialTripletGrid`

- Samples barycentric weights with presence flags.
- Handles missing neighboring cells at field boundaries.
- Supports rasterization of partial vertex neighborhoods.
