# Chromatic Grids

Chromatic grids sample chromatic values around hex vertices.

## `ChromaticIndexTripletGrid`

- Samples chromatic index triplets.
- Uses the same vertex triplet order as topology grids.
- Supports grid-to-raster conversion.

```csharp
var grid = new ChromaticIndexTripletGrid(
    hexWidth: 5,
    hexHeight: 4,
    layout: Layout.OddR,
    hexOrigin: VectorXY.Zero,
    resolution: new VectorXYInt(192, 192));

SpatialRaster<RGBA16BitColor> raster = grid.ToRGBA16BitRaster(ToColor);

raster.SaveAsPng("chromatic-index-triplet-grid-odd-r-rgba16.png");

static RGBA16BitColor ToColor(Triplet<byte> chromatic)
{
    return new RGBA16BitColor(
        ToChannel(0.18f + 0.34f * chromatic.Main),
        ToChannel(0.18f + 0.34f * chromatic.Left),
        ToChannel(0.18f + 0.34f * chromatic.Right),
        ushort.MaxValue);
}

static ushort ToChannel(float value)
{
    value = MathF.Min(MathF.Max(value, 0f), 1f);
    return (ushort)MathF.Round(value * ushort.MaxValue);
}
```

![ChromaticIndexTripletGrid rasterized with chromatic triplet colors](../../assets/hexes/grids/chromatic-index-triplet-grid-odd-r-rgba16.png)

## `ChromaticIndexPartialTripletGrid`

- Samples chromatic index triplets with presence flags.
- Handles missing neighboring cells at field boundaries.
- Supports partial triplet rasterization.
