# IntPointInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.IntPointInfluenceField> задаёт целые значения в точках пространства. Если значения обозначают районы, выберите семплер ближайшего источника. Тогда каждый запрос вернёт идентификатор одного из заданных районов.

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

Ближайшая к `(7, 1)` метка расположена в `(8, 0)`, поэтому запрос возвращает район 30. `DistinctValues` содержит идентификаторы меток, а `Min` и `Max` задают их числовые границы.
