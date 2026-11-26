# FloatCurveInfluenceField

<xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceField> uses parameterized curves as sources. A source projects the query point onto its curve and can compute a value from the coordinate along that curve. This road starts at elevation 100 and rises by 2 per unit of distance along the segment.

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

The projection of `(3, 4)` onto the road is `(3, 0)`, three units from its start. The value provider returns `100 + 2 × 3 = 106`. The constructor takes the output bounds explicitly, and the field clamps sampled values to that range.
