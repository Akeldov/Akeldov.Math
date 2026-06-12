# Sampling Strategies

Sampling defines how selected sources contribute to the final value at a point.

The examples below use the same source shape:

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

## Nearest Sampling

Nearest sampling returns the value of the closest source.

```csharp
var sampler = new NearestFloatInfluenceSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(40f, 30f));
```

![Nearest influence sampler output](../../assets/spatial2d/influence/nearest-sampler.png)

## Inverse-Distance Weighted Sampling

Inverse-distance weighted sampling blends all selected sources, weighted by distance and source weight.

```csharp
var sampler = new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(40f, 30f));
```

![Inverse-distance weighted influence sampler output](../../assets/spatial2d/influence/inverse-distance-weighted-sampler.png)

## Barycentric Sampling

Barycentric sampling interpolates over nearby source triangles.

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(40f, 30f));
```

![Barycentric influence sampler output](../../assets/spatial2d/influence/barycentric-sampler.png)
