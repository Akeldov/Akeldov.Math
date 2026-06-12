# Point Influence Fields

Point influence fields sample values from positioned sources.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 50f)
};

var sampler = new NearestFloatInfluenceSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(40f, 30f));
```

Use point fields when source values are attached to positions.
