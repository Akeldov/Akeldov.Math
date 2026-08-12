# Influence Sources

An influence source associates a position in two-dimensional space with a numeric value. In this
step, you will place five sources inside an area measuring 100 by 70 world units.

Add the namespaces and source array to `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 25f),
    new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 50f),
    new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 75f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 100f)
};
```

The <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource> constructor accepts three
arguments:

- `weight` is the non-negative source weight used by some sampling strategies;
- `position` is the source position in world coordinates;
- `value` is the field value at the source itself.

Every source in this example has the same weight, so only its position and value affect the
result. You can later increase an individual weight to strengthen that source's contribution to
inverse-distance interpolation.

Positions must be finite, the weight cannot be negative or `NaN`, and the value cannot be `NaN`.
Geometric strategies such as triangulation also require distinct source positions.

The array is ready. Continue with [Building the Field](building-the-field.md).
