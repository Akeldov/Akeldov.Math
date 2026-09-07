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

## Shared example data

The sampler and source value types are part of the generic contract. For floating-point point
sources, all three strategies can be exchanged without changing the field type. The following
examples use three sources with equal weight `1` and the same query point `P(4, 3)`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),     // A
    new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 100f),  // B
    new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 50f)    // C
};

var query = new PointXY(4f, 3f);
var geometry = new RasterGeometry(
    new PointXY(0f, 0f),
    new VectorXY(10f, 10f),
    new VectorXYInt(400, 400));
```

Each snippet below uses these `sources`, `query`, and `geometry`. Sources are passed directly to the field,
without a spatial index: all three participate in every query, so the comparison isolates the
effect of choosing a sampler.

Every PNG below is the direct output of the `RasterizeHeatMap(geometry)` and `SaveAsPng` calls
in the examples. The library samples cell centers on a `400 × 400` grid in the square
`0 ≤ x, y ≤ 10`. Its standard temperature palette maps `0` to blue, `50` to green, and `100`
to red. Positive Y points up: source `A` is at the bottom-left corner, `B` at the bottom-right,
and `C` at the top-left. Files are saved in the application's working directory.

## Nearest-source behavior

The nearest strategy asks every selected source for its `InfluenceSample` and returns the value
whose geometric `Distance` is smallest. If two distances are equal, the source that appears first
in the selection wins. Source weight does not affect the comparison.

The generic `NearestInfluenceSampler<TSource, TValue>` works with arbitrary value types, including
`bool` and application-defined categories. The float and integer variants provide convenient
numeric specializations with the same selection behavior.

### Example: nearest-source regions

```csharp
var nearestField = new FloatPointInfluenceField(
    new NearestFloatInfluenceSampler<FloatPointInfluenceSource>(),
    sources);

float nearestValue = nearestField.Sample(query); // 0
nearestField.RasterizeHeatMap(geometry).SaveAsPng("sampling-nearest.png");
```

The distance from `P` to `A` is `5`, to `B` is `√45 ≈ 6.71`, and to `C` is `√65 ≈ 8.06`.
Source `A` is the closest, so the result is its value, `0`.

The map consists of three constant regions with values `0`, `50`, and `100`.
Crossing a boundary changes the nearest source, producing a jump to another category.
This is useful for ownership regions or assigning a location to its nearest sensor.

<img src="~/assets/spatial2d/fields/sampling-nearest.png" width="400" height="400" style="max-width: 100%; height: auto;" loading="lazy" alt="Nearest-source PNG: blue value 0 at the bottom-left, green value 50 at the top-left, and red value 100 on the right.">

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

### Example: a smooth field from three measurements

```csharp
var weightedField = new FloatPointInfluenceField(
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>(),
    sources);

float weightedValue = weightedField.Sample(query); // Approximately 44.6176
weightedField.RasterizeHeatMap(geometry).SaveAsPng("sampling-inverse-distance-weighted.png");
```

With equal source weights, the contribution weights at `P` are `1/5`, `1/√45`, and `1/√65`:

```text
f(P) = (0/5 + 100/√45 + 50/√65) / (1/5 + 1/√45 + 1/√65)
     ≈ 44.62
```

Source `A` is still the closest, but the values of `B` and `C` also contribute.
The map shows a continuous transition between measurements; at each source position, the field
returns that source's value. One use is reconstructing a temperature distribution from sensors.

<img src="~/assets/spatial2d/fields/sampling-inverse-distance-weighted.png" width="400" height="400" style="max-width: 100%; height: auto;" loading="lazy" alt="Inverse-distance-weighted PNG: a smooth blend of the three sources' blue, green, and red values.">

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

### Example: linear variation inside a triangle

```csharp
var barycentricField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    sources);

float barycentricValue = barycentricField.Sample(query); // 55
barycentricField.RasterizeHeatMap(geometry).SaveAsPng("sampling-barycentric.png");
```

Point `P(4, 3)` lies inside triangle `ABC`. Its barycentric coefficients for `A`, `B`, and `C`
are `0.3`, `0.4`, and `0.3`:

```text
P = 0.3 * A + 0.4 * B + 0.3 * C
f(P) = 0.3 * 0 + 0.4 * 100 + 0.3 * 50 = 55
```

Inside the triangle, the result is the linear field `f(x, y) = 10x + 5y`.
This is useful for interpolating height or another scalar from the vertices of a triangle mesh.
In this example, every query uses the same triangle.

Outside the triangle, the sampler extrapolates the same plane, while `FloatPointInfluenceField`
clamps the result to `[0, 100]`. This makes the upper-right corner of the map constant: at
`(10, 10)`, the raw result of `150` becomes `100`.

<img src="~/assets/spatial2d/fields/sampling-barycentric.png" width="400" height="400" style="max-width: 100%; height: auto;" loading="lazy" alt="Barycentric PNG: straight color transitions in the linear field and a red area of extrapolation clamped to 100 at the top-right.">

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
