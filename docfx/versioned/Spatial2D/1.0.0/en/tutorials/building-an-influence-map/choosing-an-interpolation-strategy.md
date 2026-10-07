# Choosing an Interpolation Strategy

A sampling strategy determines how the selected sources produce one field value. Spatial2D
provides several choices for floating-point values.

## Nearest Source

The <xref:Akeldov.Math.Spatial2D.Fields.NearestFloatInfluenceSampler`1> used in the previous step
returns the value of the nearest source. It works well for discrete zones but creates abrupt
transitions at their boundaries.

## Inverse Distance

<xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1> blends the values of
all supplied sources. A source's contribution is proportional to its weight and inversely
proportional to its distance from the sampled point:

```csharp
var sampler =
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();

var field = new FloatPointInfluenceField(sampler, sources);
```

This option produces a smooth field, but without culling it examines the entire source set for
every call to `Sample`.

## Barycentric Interpolation

For the tutorial map, use <xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1>. It
interpolates a value along a segment or across a triangle of nearby suitable sources:

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);
```

One source produces a constant value, two form a linear transition, and three define a value
plane over a triangle. With more sources, the strategy looks for a suitable triangle among the
nearest candidates. Outside the selected triangle, it may extrapolate before
`FloatPointInfluenceField` clamps the result to the `Min`–`Max` range.

Keep the barycentric version in `Program.cs`. Next, make source selection local:
[Culling Distant Sources](culling-distant-sources.md).
