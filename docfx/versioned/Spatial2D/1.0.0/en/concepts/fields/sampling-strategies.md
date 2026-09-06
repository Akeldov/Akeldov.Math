# Sampling Strategies

An influence sampler combines the values contributed by a non-empty source selection at a query
point. The field controls which sources reach the sampler; the sampler defines how those sources
become one raw value. Choosing a different strategy therefore changes the shape and continuity of
the field, not just its performance.

Built-in strategies implement
<xref:Akeldov.Math.Spatial2D.Fields.IInfluenceSampler`2>.

## Choose a strategy

| Strategy | Built-in sampler | Result | Weight usage |
|---|---|---|---|
| Nearest source | <xref:Akeldov.Math.Spatial2D.Fields.NearestInfluenceSampler`2>, <xref:Akeldov.Math.Spatial2D.Fields.NearestFloatInfluenceSampler`1>, and <xref:Akeldov.Math.Spatial2D.Fields.NearestIntInfluenceSampler`1> | The value of the geometrically nearest source; boundaries between sources are sharp | Ignored |
| Inverse-distance weighted | <xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1> | A smooth floating-point blend of all selected sources | Multiplies each source's inverse-distance contribution |
| Barycentric | <xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1> and <xref:Akeldov.Math.Spatial2D.Fields.BarycentricIntSampler`1> | Linear interpolation or extrapolation along a segment or across a triangle | Affects candidate ordering when more than three sources are supplied, but not interpolation coefficients |

Use nearest sampling for categories, ownership regions, masks, or values that must not be blended.
Use inverse-distance weighting for smooth fields in which every selected source should contribute.
Use barycentric sampling for piecewise-linear values governed by local source geometry.

## Create a sampler

The sampler and source value types are part of the generic contract. For floating-point point
sources, all three strategies can be exchanged without changing the field type:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 50f)
};

IInfluenceSampler<FloatPointInfluenceSource, float> sampler =
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();

var field = new FloatPointInfluenceField(sampler, sources);
float value = field.Sample(new PointXY(4f, 3f));
```

Replace the sampler with `NearestFloatInfluenceSampler<FloatPointInfluenceSource>` for hard
regions or `BarycentricFloatSampler<FloatPointInfluenceSource>` for linear interpolation.

## Nearest-source behavior

The nearest strategy asks every selected source for its `InfluenceSample` and returns the value
whose geometric `Distance` is smallest. If two distances are equal, the source that appears first
in the selection wins. Source weight does not affect the comparison.

The generic `NearestInfluenceSampler<TSource, TValue>` works with arbitrary value types, including
`bool` and application-defined categories. The float and integer variants provide convenient
numeric specializations with the same selection behavior.

## Inverse-distance weighting

For every selected source whose distance is greater than the geometry tolerance, the sampler
calculates:

```text
contribution weight = source weight / distance
result = sum(source value * contribution weight) / sum(contribution weight)
```

Consequently, nearby sources contribute more strongly, and increasing a source weight increases
its influence proportionally. Every sampled source must provide a finite positive weight. When a
query is effectively coincident with a source, that source's value is returned immediately so the
division remains stable.

Inverse-distance weighting is global over the source list it receives. Supplying fewer sources
through a field's selection stage turns it into a local blend and can visibly change the result.

## Barycentric behavior

The barycentric samplers adapt to the number of selected sources:

| Source count | Behavior |
|---|---|
| One | Returns that source's value. |
| Two | Interpolates along the line through their source points and extrapolates beyond the segment. Coincident points fall back to the first value. |
| Three | Uses barycentric coordinates of the source triangle. A degenerate triangle falls back to the first two sources. |
| More than three | Searches triangles among the nearest effective candidates. It prefers a triangle containing the query point, then the triangle with the smallest outside-triangle penalty; if no valid triangle exists, it uses the two nearest candidates. |

Effective candidate distance is geometric distance divided by source weight, with a small positive
floor for the divisor. Once a segment or triangle is selected, its interpolation coefficients
depend only on source points and values.

By default, the sampler examines at most `DefaultMaxCandidateSamples` candidates. Increase the
limit when a larger source neighborhood must participate:

```csharp
var sampler =
    new BarycentricFloatSampler<FloatPointInfluenceSource>(
        maxCandidateSamples: 12);
```

The limit must be at least three. Triangle search is cubic in the candidate count, so prefer a
small local source selection over an unnecessarily large limit. `BarycentricIntSampler` follows
the same geometric rules and rounds the interpolated result to the nearest integer.

## Account for output ranges and invalid input

Samplers return raw mathematical results. Segment and triangle extrapolation can produce values
outside the range of their sources. Bounded float and integer fields clamp the raw result to their
public `Min` and `Max`; a generic `InfluenceField<TSource, TValue>` does not.

Every sampler requires a non-null, non-empty source list without `null` elements and a finite query
point. Individual sources remain responsible for producing valid influence samples, while the
inverse-distance strategy adds its finite-positive-weight requirement.

For the complete source-to-field pipeline, see [Fields](../fields.md). For applied examples, see
[Build an influence map](../../how-to-guides/fields/build-an-influence-map.md) and the
[Building an influence map tutorial](../../tutorials/building-an-influence-map/index.md).
