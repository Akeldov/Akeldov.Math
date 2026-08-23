# Fields

Fields map arbitrary points in two-dimensional space to values. Use these guides to turn
scattered measurements, control points, or values attached to curves into continuous data that
can be sampled directly or rasterized into an image.

## Choose a field workflow

| Goal | Start with |
| --- | --- |
| Calculate a value from a formula or an external data source | Implement <xref:Akeldov.Math.Spatial2D.Fields.IField`1>, <xref:Akeldov.Math.Spatial2D.Fields.IFloatField>, or <xref:Akeldov.Math.Spatial2D.Fields.IIntField>. |
| Interpolate floating-point values attached to points | <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource> and <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceField>. |
| Assign integer or Boolean values from point sources | <xref:Akeldov.Math.Spatial2D.Fields.IntPointInfluenceField> or <xref:Akeldov.Math.Spatial2D.Fields.BoolPointInfluenceField>. |
| Vary a floating-point value along a path or boundary | <xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceSource> and <xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceField>. |
| Convert a field into pixels | Define a <xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> and rasterize the field at its cell centers. |

An influence-field workflow has three choices:

1. Sources define where values come from and how strongly they contribute.
2. An optional source index owns an immutable snapshot and selects local sources for each sampled point.
3. A sampler combines the selected source contributions into the field value.

The bounded floating-point and integer field types validate and clamp the sampled result to their
public value range.

## Choose how sources are combined

| Desired result | Sampler |
| --- | --- |
| Hard territories around the nearest source | <xref:Akeldov.Math.Spatial2D.Fields.NearestInfluenceSampler`2> or a typed nearest sampler. |
| A smooth blend influenced by distance and source weight | <xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1>. |
| Piecewise-linear values across local source triangles | <xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1>, usually with <xref:Akeldov.Math.Spatial2D.Fields.DelaunayInfluenceSourceIndex`1>. |

Indexed selection changes the interpolation neighborhood as well as its cost. Use
<xref:Akeldov.Math.Spatial2D.Fields.DelaunayInfluenceSourceIndex`1> for triangle-based local
interpolation or <xref:Akeldov.Math.Spatial2D.Fields.HalfPlaneInfluenceSourceIndex`1> to exclude
sources hidden behind nearer half-plane boundaries.

## How-to guides

- [Build an influence map](build-an-influence-map.md) — create weighted point sources, choose a
  sampler, apply Delaunay source indexing, rasterize the field as a heat map, and save it as PNG.

## Related documentation

- [Fields concepts](../../concepts/fields.md) explains field interfaces, source ownership,
  sampling strategies, source indexing, and curve-based influence in depth.
- [Rasterization concepts](../../concepts/rasterization.md) explains raster geometry, cell-center
  sampling, color formats, and image export.
- Browse the <xref:Akeldov.Math.Spatial2D.Fields> API reference for all field types and members.

