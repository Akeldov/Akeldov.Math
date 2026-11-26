# FloatCurveInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceField> получает значения от параметризованных кривых. Источник проецирует точку запроса на кривую и может вычислять значение по координате вдоль неё. В примере начальная высота дороги равна 100 и возрастает на 2 за каждую единицу пути.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Fields;

var road = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));
var source = new FloatCurveInfluenceSource(
    weight: 1f, curve: road, valueProvider: distance => 100f + 2f * distance);
var field = new FloatCurveInfluenceField(
    new NearestFloatInfluenceSampler<ICurveInfluenceSource<float>>(),
    new ICurveInfluenceSource<float>[] { source },
    min: 100f, max: 120f);

float elevation = field.Sample(new PointXY(3f, 4f)); // 106
```

Проекция точки `(3, 4)` на дорогу — `(3, 0)`, в трёх единицах от начала отрезка. Поэтому высота равна `100 + 2 × 3 = 106`. Границы результата задаются в конструкторе, и поле ограничивает ими результат семплирования.
