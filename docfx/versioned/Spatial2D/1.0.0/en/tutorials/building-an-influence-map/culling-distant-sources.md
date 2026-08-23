# Selecting Local Sources

Without a source index, the sampler receives every source in the field. That is acceptable for a
small array, but a local map usually needs to depend on the geometric neighborhood of a point
rather than every distant source.

Create a <xref:Akeldov.Math.Spatial2D.Fields.DelaunayInfluenceSourceIndex`1> and pass the index
itself to the field:

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var sourceIndex =
    new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sourceIndex);
```

`DelaunayInfluenceSourceIndex` copies the sources into an immutable snapshot and builds a
Delaunay triangulation from their positions up front. For a point inside the triangulated area,
it returns the vertices of the containing triangle. Outside the convex hull, it returns the
nearest hull vertex or edge. The barycentric sampler therefore usually receives one, two, or
three local sources.

Create the index once and reuse it with the field because building the triangulation costs more
than one sample. It requires at least three sources with distinct positions. If all positions are
collinear, `DelaunayInfluenceSourceIndex` automatically uses a fallback strategy that keeps no
more than two sources.

Indexed selection affects both calculation cost and field semantics. If every source must
contribute, pass `sources` directly to the field instead of creating an index. Local triangular
regions are appropriate for this map, so keep the `DelaunayInfluenceSourceIndex` version.

Continue with [Rasterizing the Field](rasterizing-the-field.md).

