# Agent Notes

## Curve Interface Model

Use these contracts when changing curve abstractions or implementations in
`Akeldov.Math.Spatial2D.Curves`.

- `ICurve` is the base one-dimensional geometry contract. It can measure point
  distance through `IPointDistanceProvider` and project a finite point. It does
  not imply intersection or fill-boundary capabilities, finite length, endpoints, traversal
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
  when only geometric traversal is required.
- `IRightwardCrossingProvider` adds the fill-rule crossing query used by contours
  and regions.
- `IContourPath` combines `IFinitePath` and `IRightwardCrossingProvider`.
  Composite contours accept this contract so every retained path is finite,
  directed, and can participate in enclosure algorithms. It does not add a
  binary-intersection capability.
- `IRayPath` is a parameterized one-endpoint curve with traversal starting at
  `Origin`. Its coordinate should increase away from `Origin`.

Current examples:

- `Line` implements `ICurve`.
- `ParameterizedLine` implements `IParameterizedCurve`.
- `Ray` implements `IRayPath`.
- `Segment` and `Arc` implement `IFiniteTwoEndpointCurve`.
- `ParameterizedSegment` and `ParameterizedArc` implement `IContourPath`.

Keep curve interfaces free of filled-area semantics. Do not add `Contains`,
`Encloses`, fill rules, or region-like behavior to curve abstractions. Closed
boundary semantics belong to contours, and filled-area semantics belong to
regions.

Closed curves that unambiguously define an inside/outside area are represented
by `IContour`, not by adding enclosing behavior to curve abstractions.

Keep concrete binary intersection operations in extension classes under
`Curves/Intersections`; curve interfaces do not expose polymorphic
ray-intersection dispatch. Supported concrete curves and contours expose ray
intersections only through extension methods. `CompositeContour` and
`ParameterizedCompositeContour` intentionally do not expose ray intersections,
because their heterogeneous `IContourPath` components have no common binary-
intersection contract. Preserve the caller-owned mutable `List<PointXY>`
contract in XML comments and implementations.

For circular curves, angles are expressed in radians by default. Angle
parameters and properties must state their units in XML comments. Non-radian
members must use an explicit suffix such as `Deg`.
