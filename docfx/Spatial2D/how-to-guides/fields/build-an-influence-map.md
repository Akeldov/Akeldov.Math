# Build an Influence Map

Use a floating-point influence field when values attached to points must be interpolated across
a two-dimensional area. This guide creates a heat map from three sources and saves it as a PNG.

## Create the influence sources

Each <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource> has a weight, a world-space
position, and a value. Use finite, distinct positions. A finite positive weight works with every
floating-point sampler.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 50f)
};
```

The last argument is the scalar value contributed by the source. The resulting field range is
derived from these values, so this example has a range of `0` through `100`.

## Build the field

Choose a sampler according to the shape required between sources:

| Sampler | Use it when |
| --- | --- |
| <xref:Akeldov.Math.Spatial2D.Fields.NearestFloatInfluenceSampler`1> | Each point should take the value of its nearest source, producing hard boundaries. |
| <xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1> | All selected sources should contribute to a smooth distance-weighted blend. Source weights must be positive. |
| <xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1> | Values should vary linearly across local source triangles. |

For barycentric interpolation, a <xref:Akeldov.Math.Spatial2D.Fields.DelaunayCuller`1> selects the
containing Delaunay triangle before each sample is evaluated:

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);
```

`DelaunayCuller` requires at least three sources with unique positions. Outside the triangulated
area it selects the nearest convex-hull vertex or edge. Omit the culler constructor argument when
the sampler should evaluate every source.

The field can be queried directly in world coordinates:

```csharp
float value = field.Sample(new PointXY(50f, 32f));
```

<xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceField> clamps sampled values to the
inclusive `field.Min` and `field.Max` range derived from the source values.

## Rasterize and save the map

Define the world-space rectangle and its pixel resolution with
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry>. Rasterization samples the field at the
center of every cell.

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    new PointXY(0f, 0f),
    new VectorXY(100f, 64f),
    new VectorXYInt(160, 96));

SpatialRaster<RGBA16BitColor> heatMap = field.RasterizeHeatMap(geometry);
heatMap.SaveAsPng("influence-heatmap.png");
```

The heat-map rasterizer maps `field.Min` to the cold end of the color scale and `field.Max` to
the hot end. `SaveAsPng` writes `influence-heatmap.png` relative to the application's working
directory.

## Complete example

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 50f)
};

var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);

var geometry = new RasterGeometry(
    new PointXY(0f, 0f),
    new VectorXY(100f, 64f),
    new VectorXYInt(160, 96));

SpatialRaster<RGBA16BitColor> heatMap = field.RasterizeHeatMap(geometry);
heatMap.SaveAsPng("influence-heatmap.png");
```

For the underlying field pipeline and other rasterization options, see [Fields](../../concepts/fields.md)
and [Rasterization](../../concepts/rasterization.md).
