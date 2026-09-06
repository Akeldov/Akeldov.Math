# Fields

A field maps any point in two-dimensional space to a value. It can represent elevation, heat,
terrain cost, a material identifier, a mask, or another quantity that varies over space. Fields
are sampled directly at `PointXY` coordinates and do not require a raster.

Field types live in the <xref:Akeldov.Math.Spatial2D.Fields> namespace.

## Start with the field contract

| Interface | Contract |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Fields.IField`1> | `Sample(point)` returns one value of `TValue` at a two-dimensional point. |
| <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> | Returns `float` values in the inclusive range from `Min` to `Max`. |
| <xref:Akeldov.Math.Spatial2D.Fields.IIntField> | Returns `int` values in the inclusive range from `Min` to `Max`. |

Implement `IField<TValue>` directly when the value comes from a formula or an external data
source. Use an influence field when discrete points or curves contribute values according to
their distance from the query point.

## Follow the influence pipeline

An influence field separates source geometry, source selection, and value combination:

```text
Retained influence sources
        |
        v
Optional source index selects a non-empty local neighborhood
        |
        v
Sampler combines the selected source values
        |
        v
Bounded field validates and clamps the public result
```

<xref:Akeldov.Math.Spatial2D.Fields.InfluenceField`2> implements the generic pipeline.
<xref:Akeldov.Math.Spatial2D.Fields.PointInfluenceField`2> and
<xref:Akeldov.Math.Spatial2D.Fields.CurveInfluenceField`2> provide contracts for point and curve
sources.

## Choose the source geometry

Each typed influence source returns an <xref:Akeldov.Math.Spatial2D.Fields.InfluenceSample`1>
containing a value, a source point, the distance to that point, and a weight.

| Geometry | Built-in sources | Use it for |
|---|---|---|
| Point | <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.IntPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.BoolPointInfluenceSource> | Sensors, settlements, control points, scattered measurements, and categorical markers |
| Parameterized curve | <xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceSource> | Roads, rivers, coastlines, paths, and other values attached along a [curve](geometry-model/curves.md) |

A point source has a fixed `Position`, value, and weight. A curve source projects the query onto
its curve; its value and weight can vary with the projected curve coordinate.

## Build a point influence field

Create the sources, choose a sampler, and pass both to the field:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(5f, 8f), 50f)
};

var sampler =
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(4f, 3f));
float minimum = field.Min; // 0
float maximum = field.Max; // 100
```

Numeric point fields derive `Min`, `Max`, and `DistinctValues` from their retained sources.
`BoolPointInfluenceField` exposes distinct Boolean values without a numeric range.

## Configure source processing

The sampler is required; a source index is optional. They answer different questions:

| Decision | Detailed guide | Summary |
|---|---|---|
| How should selected values be combined? | [Sampling Strategies](fields/sampling-strategies.md) | Choose nearest-source, inverse-distance-weighted, or barycentric behavior. |
| Which sources should participate at this point? | [Source Indexing](fields/source-indexing.md) | Use half-plane or Delaunay geometry to provide a local source neighborhood. |

Indexed selection changes the field mathematically and is not only a performance optimization.
Pass the index itself to the field so it remains the single owner of the retained source snapshot:

```csharp
var sourceIndex =
    new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
var localField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    sourceIndex);
```

## Attach influence to a curve

`FloatCurveInfluenceSource` can derive its value from the nearest curve coordinate:

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

float value = curveField.Sample(new PointXY(7f, 3f)); // 70
```

## Preserve ownership and validity

When constructed from a source collection, an influence field copies the source references and
exposes the retained structure through the read-only `InfluenceSources` property. Later structural
changes to the caller's collection do not affect the field. When constructed from an index, the
field exposes the index-owned snapshot instead.

Source collections must be non-empty and contain no `null` elements. Query points and point-source
positions must be finite. Weights must be non-negative and not `NaN`; individual sampling
strategies may impose stricter requirements.

## Rasterize a field

A field remains independent of a raster and can be sampled at arbitrary points. Rasterization
evaluates it at cell centers of a `RasterGeometry`; floating-point point fields also provide a
heat-map convenience method:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(10f, 8f),
    resolution: new VectorXYInt(160, 128));

var heatMap = field.RasterizeHeatMap(geometry);
```

Continue with:

- [Sampling Strategies](fields/sampling-strategies.md)
- [Source Indexing](fields/source-indexing.md)
- [Build an influence map](../how-to-guides/fields/build-an-influence-map.md)
- [Building an influence map tutorial](../tutorials/building-an-influence-map/index.md)
- [Rasterization](rasterization.md)
