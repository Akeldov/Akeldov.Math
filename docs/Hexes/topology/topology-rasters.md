# Topology Rasters

Topology rasters sample index relationships at regular raster coordinates.

## Triplet Rasters

- `IndexTripletRaster`.
    - Samples vertex triplets as hex indexes.
- `IndexPartialTripletRaster`.
    - Samples vertex triplets with presence flags.

```csharp
var hexMapGeometry = new HexMapGeometry(5, 4, 1f, Layout.OddR);
var rasterGeometry = new RasterGeometry(
    new PointXY(0f, 0f),
    hexMapGeometry.GetBoundingBoxSize(),
    new VectorXYInt(192, 192));
var sourceRaster = new IndexTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng("index-triplet-raster-odd-r-rgba16.png");

static RGBA16BitColor ToColor(Triplet<VectorXYInt> triplet)
{
    return new RGBA16BitColor(
        ToChannel(EncodeIndex(triplet.Main)),
        ToChannel(EncodeIndex(triplet.Left)),
        ToChannel(EncodeIndex(triplet.Right)),
        ushort.MaxValue);
}

static float EncodeIndex(VectorXYInt index)
{
    return 0.08f + 0.075f * (index.X + 1) + 0.12f * (index.Y + 1);
}

static ushort ToChannel(float value)
{
    value = MathF.Min(MathF.Max(value, 0f), 1f);
    return (ushort)MathF.Round(value * ushort.MaxValue);
}
```

![IndexTripletRaster rasterized with index-derived colors](../../assets/hexes/rasters/index-triplet-raster-odd-r-rgba16.png)

## Septuplet Rasters

- `IndexSeptupletRaster`.
    - Samples full neighborhood septuplets.
- `IndexPartialSeptupletRaster`.
    - Samples partial neighborhood septuplets with presence flags.

## Rasterization Support

- Topology rasters can feed topology rasterization.
- Raster helpers can map index relationships into image-space samples.
