# Curves

Curves describe one-dimensional geometry in 2D space: infinite lines, half-lines, bounded segments, and arcs.
They live in the `Akeldov.Math.Spatial2D.Curves` namespace and are used by contours, regions, fields, and rasterizers.

Every `ICurve` can measure point distance and project a point onto itself. Supported concrete curve types
report binary intersections through extension methods, while contour paths and contours expose
polymorphic ray intersections through `IRayIntersectionProvider`.
Parameterized curves additionally expose a length-based curve coordinate through `GetPoint` and `ProjectWithParameter`.
For open polylines, [`ParameterizedSegmentChain`](linear/parameterized-segment-chain.md) composes consecutive directed segments behind one finite-path API.

Angles are expressed in radians by default. Degree-based members use the `Deg` suffix.

The table thumbnails are produced by the curve rasterizers.
Non-parameterized thumbnails show distance rasters; parameterized thumbnails show curve-coordinate-driven growing thickness.

| <span class="curve-overview-heading">Non-Parameterized</span> | Parameterized | <span class="curve-coordinate-domain">Coordinate Domain</span> | Notes |
|---|---|---|---|
| <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Line distance raster" src="../../assets/spatial2d/curves/line-distance.png"><br><span class="curve-overview-link">[`Line`](linear/line.md)</span> | <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Parameterized line growing-thickness raster" src="../../assets/spatial2d/curves/parameterized-line-growing-thickness.png"><br><span class="curve-overview-link">[`ParameterizedLine`](linear/parameterized-line.md)</span> | <span class="curve-coordinate-domain">`(-inf, +inf)`</span> | Infinite line; parameterized version adds origin and direction. |
| - | <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Ray growing-thickness raster" src="../../assets/spatial2d/curves/ray-growing-thickness.png"><br><span class="curve-overview-link">[`Ray`](linear/ray.md)</span> | <span class="curve-coordinate-domain">`[0, +inf)`</span> | Half-line, inherently directed from its origin. |
| <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Segment distance raster" src="../../assets/spatial2d/curves/segment-distance.png"><br><span class="curve-overview-link">[`Segment`](linear/segment.md)</span> | <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Parameterized segment growing-thickness raster" src="../../assets/spatial2d/curves/parameterized-segment-growing-thickness.png"><br><span class="curve-overview-link">[`ParameterizedSegment`](linear/parameterized-segment.md)</span> | <span class="curve-coordinate-domain">`[0, Length]`</span> | `Segment` is endpoint-order agnostic; `ParameterizedSegment` has start/end direction. |
| <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Arc distance raster" src="../../assets/spatial2d/curves/arc-distance.png"><br><span class="curve-overview-link">[`Arc`](circular/arc.md)</span> | <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Parameterized arc growing-thickness raster" src="../../assets/spatial2d/curves/parameterized-arc-growing-thickness.png"><br><span class="curve-overview-link">[`ParameterizedArc`](circular/parameterized-arc.md)</span> | <span class="curve-coordinate-domain">`[0, Length]`</span> | Bounded angular span; parameterized version adds traversal direction. |
| - | <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Quadratic Bezier growing-thickness raster" src="../../assets/spatial2d/curves/quadratic-bezier-growing-thickness.png"><br><span class="curve-overview-link">[`QuadraticBezier`](bezier/quadratic-bezier.md)</span> | <span class="curve-coordinate-domain">`[0, Length]`</span> | TrueType-style Bezier segment with one control point. |
| - | <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Cubic Bezier growing-thickness raster" src="../../assets/spatial2d/curves/cubic-bezier-growing-thickness.png"><br><span class="curve-overview-link">[`CubicBezier`](bezier/cubic-bezier.md)</span> | <span class="curve-coordinate-domain">`[0, Length]`</span> | Four-point Bezier segment commonly used by vector drawing formats. |

`Line` and `ParameterizedLine` can also be constructed from a point and direction angle in radians.
Bezier curve types implement `IFinitePath`, so they can be used anywhere a directed bounded curve is expected.
Their `GetPointAt` methods use the normalized Bezier parameter `t` in the `[0, 1]` range, while `GetPoint`
uses the library's length-based curve coordinate.

## Topics

- [Linear Curves](linear/index.md)
- [Circular Curves](circular/index.md)
- [Bezier Curves](bezier/index.md)
- [Curve Interfaces](curve-interfaces.md)
- [Projections and Distances](projections-and-distances.md)
- [Intersections](intersections.md)
