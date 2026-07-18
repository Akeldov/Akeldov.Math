# Geometry Rasters

Geometry rasters sample Spatial2D values from a hex field.

## `BarycentricTripletRaster`

- Samples barycentric weights for vertex triplets.
- Supports geometry rasterization workflows.
- Exposes spatial raster geometry and sampled values.

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

![BarycentricTripletRaster rasterized with the main barycentric weight](../../assets/hexes/rasters/barycentric-triplet-raster-main-odd-r-rgba16.png)

## `BarycentricPartialTripletRaster`

- Samples barycentric weights with presence flags.
- Handles missing neighboring cells at field boundaries.
- Supports rasterization of partial vertex neighborhoods.
