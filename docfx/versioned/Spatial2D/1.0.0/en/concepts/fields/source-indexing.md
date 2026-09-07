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

## Shared example data

Compare both indexes using the same five sources. The layout and colors come from the old wiki
example, with the resolution increased to `400 × 280`. Each following snippet uses these
`grid`, `sources`, `sourceColors`, and `query` variables:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(100f, 70f),
    resolution: new VectorXYInt(400, 280));

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 0f),    // A
    new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 25f),   // B
    new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 50f),   // C
    new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 75f),   // D
    new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 100f)   // E
};

var sourceColors = new Dictionary<PointXY, RGBA16BitColor>
{
    { sources[0].Position, new RGBA16BitColor(0xefef, 0x4444, 0x4444, 0xffff) }, // Red
    { sources[1].Position, new RGBA16BitColor(0x2222, 0xc5c5, 0x5e5e, 0xffff) }, // Green
    { sources[2].Position, new RGBA16BitColor(0x3b3b, 0x8282, 0xf6f6, 0xffff) }, // Blue
    { sources[3].Position, new RGBA16BitColor(0xf5f5, 0x9e9e, 0x0b0b, 0xffff) }, // Orange
    { sources[4].Position, new RGBA16BitColor(0xa8a8, 0x5555, 0xf7f7, 0xffff) }  // Purple
};
var query = new PointXY(50f, 5f);
```

Both PNGs are produced directly by the library through `RasterizeCullingMap` and `SaveAsPng`,
without post-processing. At each cell center, the rasterizer calls `SelectSources` and averages
the selected sources' assigned colors in linear RGB. **This is a source-selection map, not a
heat map of field values**: neither the values `0…100` nor a field sampler participates in
rendering it. A sharp color boundary indicates a change of selection, not necessarily a
discontinuity in the field value.

Positive Y points up in the images. Sources `A` and `B` are near the bottom, `C` and `D` near
the top, and `E` near the center. Files are saved in the application's working directory.

## Example: half-plane culling

```csharp
var halfPlaneIndex =
    new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

var halfPlaneSelection = halfPlaneIndex.SelectSources(query); // Sources A, B, E
halfPlaneIndex
    .RasterizeCullingMap(grid, point => sourceColors[point])
    .SaveAsPng("indexing-half-plane.png");
```

At `P(50, 5)`, the nearest source `E(50, 34)` is accepted first. Its boundary `y = 34` hides
`C` and `D` above it. Sources `A` and `B` remain visible, so the bottom-center region of the
map blends red, green, and purple.

Moving the query changes the traversal order and the hidden sources. Regions of constant
selection therefore need not match Delaunay triangles. This index is useful, for example,
for local blending where nearer sensors should hide more distant ones.

<img src="~/assets/spatial2d/fields/indexing-half-plane.png" width="400" height="280" style="max-width: 100%; height: auto;" loading="lazy" alt="Half-plane culling map: polygonal regions blend the visible source colors; the bottom-center selection contains A, B, and E.">

## Example: Delaunay neighborhoods

```csharp
var delaunayIndex =
    new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

var edgeSelection = delaunayIndex.SelectSources(query);                  // A, B
var triangleSelection = delaunayIndex.SelectSources(new PointXY(50f, 20f)); // A, B, E
var vertexSelection = delaunayIndex.SelectSources(new PointXY(0f, 0f));   // A
delaunayIndex
    .RasterizeCullingMap(grid, point => sourceColors[point])
    .SaveAsPng("indexing-delaunay.png");
```

Inside triangle `ABE`, for example at `(50, 20)`, the index selects its three vertices. The
color is constant throughout that triangle's interior: the map averages three colors rather
than interpolating them using barycentric coordinates.

Point `P(50, 5)` is outside the convex hull, closest to edge `AB`. Here the index selects only
`A` and `B`: the bottom strip blends red and green without the purple of `E`. At `(0, 0)`,
the nearest hull feature is vertex `A`, so the bottom-left corner is red. The source names
in the comments describe membership, not a required list order.

<img src="~/assets/spatial2d/fields/indexing-delaunay.png" width="400" height="280" style="max-width: 100%; height: auto;" loading="lazy" alt="Delaunay selection map: constant-color triangles inside the hull and regions selecting two edge vertices or one nearest vertex outside it.">

## Create and reuse an index

The map above visualizes only the index. To produce a numeric field, pass the existing index
and a sampler to the field constructor:

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, delaunayIndex);

float value = field.Sample(new PointXY(50f, 20f));
```

Unlike a constant-color triangle in the selection map, this field varies linearly between
its vertex values. Similarly, pass `halfPlaneIndex` to a field with an
`InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>` for local weighted blending.
For sampler examples, see [Sampling strategies](sampling-strategies.md).

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
