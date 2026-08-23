# Fields

A field maps a point in two-dimensional space to a value. Fields can represent heat, elevation,
terrain cost, material identifiers, masks, or any other value that varies over space. Sampling a
field does not require a raster: the caller supplies a `PointXY` and receives a value directly.

Field types live in the <xref:Akeldov.Math.Spatial2D.Fields> namespace.

## Understand the field interfaces

The base interface is deliberately small:

| Interface | Contract |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Fields.IField`1> | `Sample(point)` returns one value of `TValue` at a two-dimensional point. |
| <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> | Samples `float` values in the inclusive range from `Min` to `Max`. |
| <xref:Akeldov.Math.Spatial2D.Fields.IIntField> | Samples `int` values in the inclusive range from `Min` to `Max`. |

Implement `IField<TValue>` directly when a value has a closed-form expression or comes from an
external data source:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

public sealed class HorizontalGradientField : IFloatField
{
    public float Min => 0f;
    public float Max => 100f;

    public float Sample(PointXY point)
    {
        float value = point.X;
        return value.Clamp(Min, Max);
    }
}
```

Use an influence field when values should be derived from discrete point or curve sources.

## Follow the influence pipeline

An influence field separates four responsibilities:

```text
Field-owned sources or an index-owned source snapshot
        |
        v
Optional source index selects a non-empty local subset
        |
        v
Sampler combines source contributions into a raw value
        |
        v
Bounded field validates and clamps the public result
```

This separation lets the same logical source set use nearest-neighbor, inverse-distance, or
barycentric sampling, with or without indexed geometric selection.

<xref:Akeldov.Math.Spatial2D.Fields.InfluenceField`2> implements the generic pipeline.
<xref:Akeldov.Math.Spatial2D.Fields.PointInfluenceField`2> and
<xref:Akeldov.Math.Spatial2D.Fields.CurveInfluenceField`2> constrain it to point and curve
sources.

## Choose influence sources

Every <xref:Akeldov.Math.Spatial2D.Fields.IInfluenceSource> measures distance to a sampled point.
A typed source returns an <xref:Akeldov.Math.Spatial2D.Fields.InfluenceSample`1> containing:

- the contributed value;
- the source point used for the contribution;
- the non-negative distance to that point;
- the source weight used by compatible samplers.

Choose a source from where the value is attached:

| Source geometry | Built-in sources | Behavior |
|---|---|---|
| Point | <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.IntPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.BoolPointInfluenceSource> | The source point is its fixed `Position`; value and weight are constant. |
| Parameterized curve | <xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceSource> | The source point is the projection onto the curve; value and weight may vary with the curve coordinate. |

Point sources are suitable for settlements, sensors, control handles, or scattered measurements.
Curve sources are suitable for roads, rivers, coastlines, paths, and other features whose
nearest position can lie anywhere along a [curve](geometry-model/curves.md).

## Build a point influence field

The following field returns a floating-point value derived from three positioned sources:

```csharp
var sources = new[]
{
    new FloatPointInfluenceSource(
        weight: 1f,
        position: new PointXY(0f, 0f),
        value: 0f),
    new FloatPointInfluenceSource(
        weight: 1f,
        position: new PointXY(10f, 0f),
        value: 100f),
    new FloatPointInfluenceSource(
        weight: 1f,
        position: new PointXY(5f, 8f),
        value: 50f)
};

var sampler =
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();

var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(4f, 3f));
float minimum = field.Min; // 0
float maximum = field.Max; // 100
```

`FloatPointInfluenceField` derives `Min`, `Max`, and `DistinctValues` from the copied source set.
`IntPointInfluenceField` does the same for integer sources. `BoolPointInfluenceField` exposes
distinct Boolean values but has no numeric range.

## Choose a sampling strategy

The sampler determines how the selected sources contribute to the result:

| Strategy | Built-in sampler | Result |
|---|---|---|
| Nearest | <xref:Akeldov.Math.Spatial2D.Fields.NearestInfluenceSampler`2> and numeric specializations | Returns the value from the source with the smallest geometric distance. Works with arbitrary value types. |
| Inverse-distance weighted | <xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1> | Blends floating-point values using `source weight / distance`; an effectively coincident source wins immediately. |
| Barycentric | <xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1> and <xref:Akeldov.Math.Spatial2D.Fields.BarycentricIntSampler`1> | Interpolates or extrapolates along a segment or across a source triangle, with fallbacks for degenerate layouts. |

Nearest sampling creates piecewise-constant territories. Inverse-distance weighting creates a
smooth global blend. Barycentric sampling creates piecewise linear variation governed by nearby
source geometry.

A sampler is a mathematical strategy and may extrapolate outside the source value range.
Bounded field types preserve their public contract by clamping the raw result. Generic
`InfluenceField<TSource, TValue>` does not add a range or clamp the sampler result.

Inverse-distance weighting requires every sampled influence to have a finite positive weight.
Nearest sampling ignores weight. Barycentric candidate selection uses weight-adjusted effective
distance, while the interpolation itself uses source positions and values.


## Index sources before sampling

A source index owns an immutable snapshot of the sources and selects the sources relevant at each
point before the sampler runs. Indexed selection changes the local interpolation neighborhood; it
is not only a performance switch.

Built-in indexes operate on point influence sources:

| Source index | Selection behavior |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Fields.HalfPlaneInfluenceSourceIndex`1> | Visits sources from nearest to farthest and excludes sources hidden behind half-plane boundaries introduced by nearer sources. |
| <xref:Akeldov.Math.Spatial2D.Fields.DelaunayInfluenceSourceIndex`1> | Returns the containing Delaunay triangle; outside the triangulation it returns the nearest convex-hull vertex or edge, with a collinear fallback. |

`DelaunayInfluenceSourceIndex` requires at least three sources with unique positions.
Non-collinear sources are triangulated when the index is constructed. Both built-in indexes
expose their retained snapshot through the read-only `Sources` property and return a new mutable,
non-empty selection list owned by the caller.

Pass the source index itself to the field; the index is the single owner of the retained source
snapshot:

```csharp
var sourceIndex =
    new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
var barycentric = new BarycentricFloatSampler<FloatPointInfluenceSource>();

var localField = new FloatPointInfluenceField(
    barycentric,
    sourceIndex);

float localValue = localField.Sample(new PointXY(4f, 3f));
```

Custom implementations of <xref:Akeldov.Math.Spatial2D.Fields.IInfluenceSourceIndex`1> must expose
a non-empty source snapshot and return at least one source from that snapshot. A `null` or empty
selection, or a selection containing `null`, makes the field fail explicitly. Returning a source
outside the snapshot violates the index contract.

## Attach influence to a curve

`FloatCurveInfluenceSource` wraps an `IParameterizedCurve`. For each sample it projects the point
onto the curve and evaluates its value and weight providers at the projected curve coordinate:

```csharp
using Akeldov.Math.Spatial2D.Curves;

var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

var pathSource = new FloatCurveInfluenceSource(
    weight: 1f,
    curve: path,
    valueProvider: curveCoordinate => curveCoordinate * 10f);

var curveField = new FloatCurveInfluenceField(
    new NearestFloatInfluenceSampler<ICurveInfluenceSource<float>>(),
    new ICurveInfluenceSource<float>[] { pathSource },
    min: 0f,
    max: 100f);

float curveValue = curveField.Sample(new PointXY(7f, 3f)); // 70
```

Constant-value and constant-weight overloads are available. Coordinate-based providers are
validated when sampled: weights must be non-negative and not `NaN`, and values must not be
`NaN`. `FloatCurveInfluenceField` uses the explicit `min` and `max` supplied to its constructor
and clamps results to that inclusive range.


## Preserve source ownership and validity

When an influence field is constructed from a source collection, it requires a non-empty
collection with no `null` elements, copies the source references into private storage, and exposes
that retained structure through the read-only `InfluenceSources` property. Later mutations of
the caller's list or array do not change the field's order or source count.

When the field is constructed from an `IInfluenceSourceIndex<TSource>`, the index owns the
immutable snapshot and the field exposes the same snapshot through `InfluenceSources`. The
index must isolate that snapshot from later structural changes to the caller's input.

Sampling points and point-source positions must be finite. Source weights must be non-negative
and not `NaN`; individual samplers may impose stronger rules. Floating-point source values and
bounded field ranges must not contain `NaN`.

## Rasterize a field

A field remains continuous and can be sampled at arbitrary points. Rasterization evaluates it at
the cell centers of a `RasterGeometry` and stores the mapped values. `FloatPointInfluenceField`
also provides a heat-map convenience rasterizer:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(10f, 8f),
    resolution: new VectorXYInt(160, 128));

var heatMap = field.RasterizeHeatMap(geometry);
```

For end-to-end examples, see:

- [Build an influence map](../how-to-guides/fields/build-an-influence-map.md)
- [Building an influence map tutorial](../tutorials/building-an-influence-map/index.md)
- [Rasterization](rasterization.md)

