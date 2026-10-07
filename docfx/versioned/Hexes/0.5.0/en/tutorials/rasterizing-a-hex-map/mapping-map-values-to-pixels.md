# Mapping Map Values to Pixels

Combine each index triplet with its barycentric weights. Interior pixels blend three elevations;
boundary pixels blend only the positions still present in the finite map.

## Build the elevation raster

Add this code after constructing both lookup rasters:

```csharp
var elevationValues = new float[
    checked(rasterGeometry.Resolution.X * rasterGeometry.Resolution.Y)];

for (int i = 0; i < elevationValues.Length; i++)
{
    elevationValues[i] = InterpolateElevation(
        elevationMap,
        indexRaster[i],
        barycentricRaster[i]);
}

var elevationRaster = new SpatialRaster<float>(
    rasterGeometry,
    elevationValues);

static float InterpolateElevation(
    HexMap<float> map,
    PartialTriplet<VectorXYInt> cells,
    PartialTriplet<float> weights)
{
    float weightedValue = 0f;
    float weightSum = 0f;

    if (cells.HasMain)
    {
        weightedValue += map[cells.Main] * weights.Main;
        weightSum += weights.Main;
    }

    if (cells.HasLeft)
    {
        weightedValue += map[cells.Left] * weights.Left;
        weightSum += weights.Left;
    }

    if (cells.HasRight)
    {
        weightedValue += map[cells.Right] * weights.Right;
        weightSum += weights.Right;
    }

    return weightSum > 0f
        ? weightedValue / weightSum
        : float.NaN;
}
```

Dividing by `weightSum` renormalizes a partial neighborhood, so values remain in the original
range instead of fading toward zero at an outer edge. A sample with no present cell becomes
`float.NaN`; this sentinel is safe here because source elevations are finite normalized values.

`SpatialRaster<float>` retains `rasterGeometry`, so later transformations preserve the exact
world-space placement and resolution. Its value array is newly allocated and independent of both
lookup rasters and the source map.

Continue with [Mapping Values to Colors](mapping-values-to-colors.md).
