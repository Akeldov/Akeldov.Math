# Chromatic Maps

Chromatic maps store chromatic class values by hex index.

## `ChromaticIndexMap`

- Maps hex indexes to chromatic classes.
- Implements the common hex map contract.
- Exposes dimensions and layout metadata.

```csharp
var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 8f,
    layout: Layout.OddR);
var map = new ChromaticIndexMap(geometry);

SpatialRaster<RGBA16BitColor> raster = map.Rasterize(
    pixelsPerApothem: 24f,
    margin: 0f,
    colorSelector: ToColor);

raster.SaveAsPng("chromatic-index-map-odd-r-rgba16.png");

static RGBA16BitColor ToColor(byte chromaticIndex)
{
    return chromaticIndex switch
    {
        0 => new RGBA16BitColor(0xefff, 0x4750, 0x4750, ushort.MaxValue),
        1 => new RGBA16BitColor(0x3b60, 0xc990, 0x72a0, ushort.MaxValue),
        2 => new RGBA16BitColor(0x4760, 0x77a0, 0xe8ff, ushort.MaxValue),
        _ => new RGBA16BitColor(0x2020, 0x2020, 0x2020, ushort.MaxValue)
    };
}
```

![ChromaticIndexMap rasterized with chromatic class colors](../../assets/hexes/maps/chromatic-index-map-odd-r-rgba16.png)

## Access

- Indexes by hex coordinates.
- Validates out-of-bounds access.
- Uses layout-aware chromatic class calculation.
