# Building the Field

A field combines the sources with a strategy that computes a value at any point. Start with the
simplest strategy: use the value of the nearest source.

Add this code after the `sources` array:

```csharp
var nearestSampler =
    new NearestFloatInfluenceSampler<FloatPointInfluenceSource>();

var field = new FloatPointInfluenceField(nearestSampler, sources);

float valueAtCenter = field.Sample(new PointXY(50f, 34f));
Console.WriteLine($"Value at center: {valueAtCenter}");
```

The point `(50, 34)` matches the position of the last source, so the program prints:

```text
Value at center: 100
```

<xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceField> copies the supplied list and
exposes its stored sources through the read-only `InfluenceSources` property. Changing the
original `sources` array does not change an existing field; create a new field to replace its
source set.

The field also determines the range of its source values. In this example, `field.Min` is `0`,
`field.Max` is `100`, and `field.DistinctValues` contains five values. `Sample` clamps its result
to this range even if the interpolation strategy calculates a value outside it.

The current map will contain sharp regions around the sources. In the next step, you will replace
the sampler to create smooth transitions: [Choosing an Interpolation Strategy](choosing-an-interpolation-strategy.md).
