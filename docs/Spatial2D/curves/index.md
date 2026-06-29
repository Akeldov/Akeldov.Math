# Curves

Curves describe one-dimensional geometry in 2D space: infinite lines, half-lines, bounded segments, and arcs.
They live in the `Akeldov.Math.Spatial2D.Curves` namespace and are used by contours, regions, fields, and rasterizers.

Every curve can measure point distance, project a point onto itself, and report intersections with a ray.
Parameterized curves additionally expose a length-based curve coordinate through `GetPoint` and `ProjectWithParameter`.
For open polylines, `ParameterizedSegmentChain` composes consecutive directed segments behind one finite-path API.

Angles are expressed in radians by default. Degree-based members use the `Deg` suffix.

The table thumbnails are produced by the curve rasterizers.
Non-parameterized thumbnails show distance rasters; parameterized thumbnails show curve-coordinate-driven growing thickness.

| <span class="curve-overview-heading">Non-Parameterized</span> | Parameterized | <span class="curve-coordinate-domain">Coordinate Domain</span> | Notes |
|---|---|---|---|
| <img class="curve-overview-thumbnail" alt="Line distance raster" src="../../assets/spatial2d/curves/line-distance.png"><br><span class="curve-overview-link">[`Line`](linear/line.md)</span> | <img class="curve-overview-thumbnail" alt="Parameterized line growing-thickness raster" src="../../assets/spatial2d/curves/parameterized-line-growing-thickness.png"><br><span class="curve-overview-link">[`ParameterizedLine`](linear/parameterized-line.md)</span> | <span class="curve-coordinate-domain">`(-inf, +inf)`</span> | Infinite line; parameterized version adds origin and direction. |
| - | <img class="curve-overview-thumbnail" alt="Ray growing-thickness raster" src="../../assets/spatial2d/curves/ray-growing-thickness.png"><br><span class="curve-overview-link">[`Ray`](linear/ray.md)</span> | <span class="curve-coordinate-domain">`[0, +inf)`</span> | Half-line, inherently directed from its origin. |
| <img class="curve-overview-thumbnail" alt="Segment distance raster" src="../../assets/spatial2d/curves/segment-distance.png"><br><span class="curve-overview-link">[`Segment`](linear/segment.md)</span> | <img class="curve-overview-thumbnail" alt="Parameterized segment growing-thickness raster" src="../../assets/spatial2d/curves/parameterized-segment-growing-thickness.png"><br><span class="curve-overview-link">[`ParameterizedSegment`](linear/parameterized-segment.md)</span> | <span class="curve-coordinate-domain">`[0, Length]`</span> | `Segment` is endpoint-order agnostic; `ParameterizedSegment` has start/end direction. |
| <img class="curve-overview-thumbnail" alt="Arc distance raster" src="../../assets/spatial2d/curves/arc-distance.png"><br><span class="curve-overview-link">[`Arc`](circular/arc.md)</span> | <img class="curve-overview-thumbnail" alt="Parameterized arc growing-thickness raster" src="../../assets/spatial2d/curves/parameterized-arc-growing-thickness.png"><br><span class="curve-overview-link">[`ParameterizedArc`](circular/parameterized-arc.md)</span> | <span class="curve-coordinate-domain">`[0, Length]`</span> | Bounded angular span; parameterized version adds traversal direction. |

`Line` and `ParameterizedLine` can also be constructed from a point and direction angle in radians.

## Topics

- [Curve Interfaces](curve-interfaces.md)
- [Projections and Distances](projections-and-distances.md)
- [Intersections](intersections.md)
