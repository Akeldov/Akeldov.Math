# Rasters

Rasters are spatially sampled value containers used by geometry, topology, and chromatization APIs.

## Topics

- [`ISpatialRaster<TValue>`](rasters/ispatialraster.md)
- [Shared Raster Behavior](rasters/shared-raster-behavior.md)
- [Rasterization Integration](rasters/rasterization-integration.md)

## Example Outputs

Topology rasters can visualize sampled index relationships.

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

![IndexTripletRaster rasterized with index-derived colors](../assets/hexes/rasters/index-triplet-raster-odd-r-rgba16.png)

Geometry rasters can visualize sampled barycentric weights.

```csharp
var hexGeometry = new HexMapGeometry(5, 4, VectorXY.Zero, 1f, Layout.OddR);
var rasterGeometry = new RasterGeometry(
    new PointXY(-4f, -4f),
    new VectorXY(8f, 8f),
    new VectorXYInt(192, 192));
var sourceRaster = new BarycentricTripletRaster(hexGeometry, rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng("barycentric-triplet-raster-main-odd-r-rgba16.png");

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

![BarycentricTripletRaster rasterized with the main barycentric weight](../assets/hexes/rasters/barycentric-triplet-raster-main-odd-r-rgba16.png)

Chromatic rasters can visualize sampled chromatic triplets.

```csharp
var topology = new HexMapTopology(5, 4, Layout.OddR);
var hexMapGeometry = new HexMapGeometry(topology, 1f);
var rasterGeometry = new RasterGeometry(
    new PointXY(0f, 0f),
    hexMapGeometry.GetBoundingBoxSize(),
    new VectorXYInt(192, 192));
var sourceRaster = new ChromaticIndexTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng("chromatic-index-triplet-raster-odd-r-rgba16.png");

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

![ChromaticIndexTripletRaster rasterized with chromatic triplet colors](../assets/hexes/rasters/chromatic-index-triplet-raster-odd-r-rgba16.png)
