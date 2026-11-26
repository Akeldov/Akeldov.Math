# FloatPointInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceField> оценивает числовое значение по измерениям в известных точках. В примере два датчика с одинаковым весом показывают 12 и 20 градусов. На равном расстоянии от них семплер с обратным взвешиванием по расстоянию учитывает оба показания поровну.

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

Границы поля определяются значениями источников. Здесь `Min` равен 12, а `Max` — 20. В `DistinctValues` входят два показания датчиков, а между датчиками можно получить и промежуточную температуру.
