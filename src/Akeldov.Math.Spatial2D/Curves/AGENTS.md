# Agent Notes

## Curve Interface Model

Use these contracts when changing curve abstractions or implementations in
`Akeldov.Math.Spatial2D.Curves`.

- `ICurve` is the base one-dimensional geometry contract. It can measure point
  distance through `IPointDistanceProvider`, project a finite point, and report
  ray intersections. It does not imply finite length, endpoints, traversal
  direction, parameterization, closure, or inside/outside semantics.
- `IFiniteCurve` adds a finite non-negative `Length` in world coordinate units.
  It does not imply endpoints or traversal direction.
- `IOneEndpointCurve` adds a single `Endpoint`. It does not imply
  parameterization or traversal direction by itself.
- `ITwoEndpointCurve` adds unordered `EndpointA` and `EndpointB`. These are
  boundary points, not start/end points. Use `IPath` when traversal direction is
  part of the contract.
- `IFiniteTwoEndpointCurve` combines finite length with unordered two-endpoint
  geometry.
- `IParameterizedCurve` adds `GetPoint` and `ProjectWithParameter`. Curve
  coordinates are measured in world coordinate units along the curve and are not
  normalized to `[0, 1]`. Each implementation must define and document its valid
  coordinate domain.
- `IPath` is a parameterized two-endpoint curve with explicit traversal
  direction. `StartPoint` and `EndPoint` are directional and must be consistent
  with increasing curve coordinates.
- `IFinitePath` is a finite directed path. It is suitable for contour segments,
  because contours require finite curves with start and end points.
- `IRayPath` is a parameterized one-endpoint curve with traversal starting at
  `Origin`. Its coordinate should increase away from `Origin`.

Current examples:

- `Line` implements `ICurve`.
- `ParameterizedLine` implements `IParameterizedCurve`.
- `Ray` implements `IRayPath`.
- `Segment` and `Arc` implement `IFiniteTwoEndpointCurve`.
- `ParameterizedSegment` and `ParameterizedArc` implement `IFinitePath`.

Keep curve interfaces free of filled-area semantics. Do not add `Contains`,
`Encloses`, fill rules, or region-like behavior to curve abstractions. Closed
boundary semantics belong to contours, and filled-area semantics belong to
regions.

Closed curves that unambiguously define an inside/outside area are represented
by `IContour`, not by adding enclosing behavior to curve abstractions.

`ICurve.GetPointIntersections` returns a new mutable `List<PointXY>` owned by the
caller. Preserve that ownership contract in XML comments and implementations.

For circular curves, angles are expressed in radians by default. Angle
parameters and properties must state their units in XML comments. Non-radian
members must use an explicit suffix such as `Deg`.
