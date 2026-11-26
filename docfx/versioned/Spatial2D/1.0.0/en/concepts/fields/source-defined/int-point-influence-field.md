# IntPointInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.IntPointInfluenceField> assigns integer values to positions. For district IDs, use a nearest-source sampler so each query returns the ID of an existing marker.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new IntPointInfluenceSource(1f, new PointXY(0f, 0f), 10),
    new IntPointInfluenceSource(1f, new PointXY(8f, 0f), 30)
};
var sampler = new NearestIntInfluenceSampler<IntPointInfluenceSource>();
var field = new IntPointInfluenceField(sampler, sources);

int districtId = field.Sample(new PointXY(7f, 1f)); // 30
```

The marker at `(8, 0)` is closer to `(7, 1)`, so the query returns district 30. `DistinctValues` lists the marker IDs. `Min` and `Max` give their numeric bounds.
