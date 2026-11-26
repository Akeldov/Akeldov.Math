# BoolPointInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.BoolPointInfluenceField> задаёт логическую маску по точечным источникам. В примере каждый источник обозначает ворота, а маска показывает, открыты ли ближайшие к точке запроса ворота.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new BoolPointInfluenceSource(1f, new PointXY(0f, 0f), true),
    new BoolPointInfluenceSource(1f, new PointXY(8f, 0f), false)
};
var sampler = new NearestInfluenceSampler<BoolPointInfluenceSource, bool>();
var field = new BoolPointInfluenceField(sampler, sources);

bool nearOpenGate = field.Sample(new PointXY(1f, 2f));   // true
bool nearClosedGate = field.Sample(new PointXY(7f, 2f)); // false
```

В этом примере граница между значениями маски проходит по прямой `x = 4`, равноудалённой от обоих источников. В `DistinctValues` входят состояния, представленные среди источников. Если все ворота открыты, там будет только `true`.
