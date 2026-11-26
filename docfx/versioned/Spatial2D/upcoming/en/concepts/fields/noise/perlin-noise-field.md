# PerlinNoiseField

<xref:Akeldov.Math.Spatial2D.Fields.PerlinNoiseField> computes a normalized noise value at any
supported world-space point. Its `Min` is 0 and its `Max` is 1.

## Sample terrain height

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

IFloatField terrain = new PerlinNoiseField(seed: 42, scale: 32f);
float height = terrain.Sample(new PointXY(12.5f, -4f));
```

Here `height` is a fraction of the elevation range. For terrain between 100 and 250 meters,
convert it with `100f + 150f * height`. Query the same field across adjacent map tiles using
world coordinates to keep the terrain continuous at their edges.

## Control the pattern

| Parameter | Effect |
|---|---|
| `seed` | Selects a repeatable arrangement of noise gradients. |
| `scale` | Sets the base feature size in world units. Larger values give broader features. |
| `octaves` | Sets the number of layers. The default is 4. |
| `persistence` | Multiplies the amplitude of each successive layer. The default is 0.5. Set it to 0 to use just the first layer. |
| `lacunarity` | Multiplies the frequency of each successive layer. The default of 2 doubles the frequency at each step. |
| `offset` | Adds a displacement to the query point before sampling. The default is the zero vector. |

To generate a whole grid, `RasterGeometry.CreatePerlinNoise` samples the same noise at cell
centers using the same settings. See [SpatialFloatRaster](../../rasterization/spatial-rasters/spatial-float-raster.md).
