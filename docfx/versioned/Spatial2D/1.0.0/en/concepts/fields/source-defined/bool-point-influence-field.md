# BoolPointInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.BoolPointInfluenceField> builds a Boolean mask from point sources. Each source in this example marks a gate, and the mask reports whether the nearest gate is open.

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

With these two gates the mask changes value halfway between them, at `x = 4`. `DistinctValues` contains the Boolean states present among the sources. If every gate is open, it contains only `true`.
