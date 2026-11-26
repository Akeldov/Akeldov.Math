# FloatPointInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceField> estimates a numeric value from measurements at known positions. Here two sensors with equal weights report 12 and 20 degrees. The inverse-distance sampler gives them equal influence halfway between them.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 12f),
    new FloatPointInfluenceSource(1f, new PointXY(8f, 0f), 20f)
};
var sampler = new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);

float temperature = field.Sample(new PointXY(4f, 0f)); // 16
```

The field derives its inclusive range from the sources. Here `Min` is 12 and `Max` is 20. `DistinctValues` contains the two sensor readings, while sampling between the sensors can also yield intermediate temperatures.
