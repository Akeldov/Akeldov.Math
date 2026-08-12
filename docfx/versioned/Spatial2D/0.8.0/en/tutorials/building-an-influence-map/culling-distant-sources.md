# Culling Distant Sources

Without culling, the sampler receives every source in the field. That is acceptable for a small
array, but a local map usually needs to depend on the geometric neighborhood of a point rather
than every distant source.

Create a <xref:Akeldov.Math.Spatial2D.Fields.DelaunayCuller`1> and pass it as the third field
constructor argument:

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);
```

`DelaunayCuller` builds a Delaunay triangulation from the source positions up front. For a point
inside the triangulated area, it returns the vertices of the containing triangle. Outside the
convex hull, it returns the nearest hull vertex or edge. The barycentric sampler therefore
usually receives one, two, or three local sources.

Create the culler once and reuse it with the field because building the triangulation costs more
than one sample. It requires at least three sources with distinct positions. If all positions are
collinear, `DelaunayCuller` automatically uses a fallback strategy that keeps no more than two
sources.

Culling affects both calculation cost and field semantics. If every source must contribute, use
the field constructor without a culler. Local triangular regions are appropriate for this map,
so keep the `DelaunayCuller` version.

Continue with [Rasterizing the Field](rasterizing-the-field.md).
