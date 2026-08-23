# Agent Notes

## Contour Interface Model

Use these contracts when changing contour abstractions or implementations in
`Akeldov.Math.Spatial2D.Contours`.

- `IContour` is a closed finite one-dimensional boundary that unambiguously
  defines an inside/outside area. It extends `IFiniteCurve`,
  `ISignedPointDistanceProvider`, `IRightwardCrossingProvider`, and
  `IRayIntersectionProvider`.
- `IContour.Encloses` tests whether a point lies inside or on the closed
  boundary. Boundary-inclusive behavior is part of the contour contract.
- `IContour.SignedDistance` is expected to be negative for points enclosed by
  the contour, positive outside, and zero on the boundary.
- `ICompositeContour` is a contour built from a structural read-only list of
  `IContourPath` curves. The paths must form a closed chain where each path
  starts at the previous path's end and the final path closes back to the first
  path.
- `IParameterizedContour` is a contour with a length-based curve coordinate. It
  inherits `IParameterizedCurve`; coordinates are measured in world coordinate
  units along the contour and are not normalized to `[0, 1]`.
- `IParameterizedCompositeContour` combines composite contour structure with
  length-based parameterization.
- `ContourDirection` describes traversal direction around a closed contour.
  Parameterized contour implementations that expose direction must keep
  `GetPoint`, `ProjectWithParameter`, and start/origin semantics consistent with
  that direction.

Current examples:

- `Circle`, `RectangleContour`, and `OrientedRectangleContour` implement
  `IContour`.
- `ParameterizedCircle`, `ParameterizedRectangleContour`, and
  `ParameterizedOrientedRectangleContour` implement `IParameterizedContour`.
- `CompositeContour` implements `ICompositeContour`.
- `ParameterizedCompositeContour` implements `IParameterizedCompositeContour`.

Keep contour interfaces boundary-focused. Do not add `Contains` or fill-rule
semantics to contour abstractions. Use `Encloses` for the inside/outside area
induced by a single contour, and use region abstractions for filled-area
membership.

When a concrete contour has a natural filled counterpart, expose conversion in
terms of the region type rather than making the contour itself a region. Current
pairs include `RectangleContour`/`Rectangle` and
`OrientedRectangleContour`/`OrientedRectangle`; keep future pairs consistent.

Use `IContourBasedRegion` or concrete `IRegion` implementations when multiple
contours, holes, or fill rules are part of the model. Fill rules belong to
regions, not to base contour abstractions.

`ICompositeContour.Curves` and `IParameterizedCompositeContour.Curves` are
read-only structural views. Implementations should copy or otherwise protect
retained input collections, and XML comments should preserve that ownership
semantics.

For circular contour members, angles are expressed in radians by default. Angle
parameters and properties must state their units in XML comments. Non-radian
members must use an explicit suffix such as `Deg`.
