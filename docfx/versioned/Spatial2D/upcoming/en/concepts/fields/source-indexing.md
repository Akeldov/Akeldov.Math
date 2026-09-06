# Source Indexing

An influence-source index selects a local source neighborhood before a field sampler combines
the source values. Indexing can reduce the number of sources considered at a point, but it is
also part of the field's mathematical definition: changing the selected neighborhood can change
the sampled value.

Built-in indexes work with point influence sources and implement
<xref:Akeldov.Math.Spatial2D.Fields.IInfluenceSourceIndex`1>.

## Understand the contract

An index has two responsibilities:

1. Retain a structurally immutable source snapshot in `Sources`.
2. Return a relevant, non-empty subset from `SelectSources(point)`.

`SelectSources` returns a new mutable list owned by the caller. Every item in that list must come
from the retained snapshot. The index, rather than the field, is responsible for a fallback when
its primary geometric selection finds no candidates.

Pass the index directly to an influence-field constructor. The field then exposes the same
snapshot through `InfluenceSources` and asks the index for a local selection on every sample:

```text
Index-owned source snapshot
        |
        v
SelectSources(point) -> local non-empty neighborhood
        |
        v
Sampler -> bounded field result
```

## Choose an index

| Index | Requirements | Sources selected at a point | Typical use |
|---|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Fields.HalfPlaneInfluenceSourceIndex`1> | One or more point sources | Sources that remain visible after nearer sources introduce perpendicular half-plane boundaries | Local nearest or weighted sampling where nearer sources should hide sources behind them |
| <xref:Akeldov.Math.Spatial2D.Fields.DelaunayInfluenceSourceIndex`1> | At least three point sources with distinct positions | Three vertices of the containing Delaunay triangle; one hull vertex or two hull-edge vertices outside the triangulation | Piecewise-linear barycentric interpolation over scattered values |

`HalfPlaneInfluenceSourceIndex` visits sources from nearest to farthest. Each accepted source
introduces a line perpendicular to the direction from the query point to that source; farther
sources on the opposite side are excluded. A source effectively coincident with the query point
is accepted but does not introduce an unstable boundary.

`DelaunayInfluenceSourceIndex` builds its triangulation once in the constructor. Inside the
triangulation it supplies one triangle to the sampler. Outside the convex hull it supplies the
nearest hull feature: one source for a vertex or two for an edge. If all positions are collinear,
it falls back to a one- or two-source linear neighborhood instead of creating triangles.

## Create and reuse an index

The following field uses one Delaunay triangle at a time for barycentric interpolation:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 50f),
    new FloatPointInfluenceSource(1f, new PointXY(10f, 10f), 75f)
};

var sourceIndex =
    new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sourceIndex);

float value = field.Sample(new PointXY(4f, 3f));
```

Create the index once and reuse it for repeated sampling. Both built-in indexes copy the source
references into their own snapshot, so adding or removing items in the original collection does
not alter the index. If the source set or source positions change, construct a new index; in
particular, a Delaunay triangulation is not updated incrementally.

## Account for boundary and degenerate cases

- Selection is always non-empty for a valid index, including queries far outside the source
  bounds.
- Delaunay interpolation uses three sources inside the triangulation, one or two outside its
  convex hull, and at most two for a collinear source layout.
- Duplicate or effectively equal positions are rejected by the Delaunay index because they do
  not define distinct triangulation vertices.
- Both built-in indexes require finite query points and finite point-source positions.
- Built-in indexes accept point sources only. Curve influence sources currently require either
  direct sampling or a custom index.

Do not add an index merely as a transparent optimization. For example, inverse-distance
weighting over all sources produces a global blend, while the same sampler behind an index blends
only the selected neighborhood. Keep direct source sampling when every source is intended to
affect every query or when the source set is already small.

For the complete field pipeline, see [Fields](../fields.md). For applied examples, see
[Build an influence map](../../how-to-guides/fields/build-an-influence-map.md) and the
[Building an influence map tutorial](../../tutorials/building-an-influence-map/index.md).
