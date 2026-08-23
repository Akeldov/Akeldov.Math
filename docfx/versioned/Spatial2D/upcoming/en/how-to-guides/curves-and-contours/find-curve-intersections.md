# Find Curve Intersections

Use the concrete `GetPointIntersections` extension methods to find isolated points where two
supported geometries meet. The ray overloads return only intersections at the ray origin or in
front of it. Curve interfaces, including `ICurve`, `IContourPath`, and `IContour`, do not declare
ray intersections.

## Cast a ray through a curve

The following ray starts to the left of a circle and points along the positive X axis, so it
crosses the boundary twice:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var boundary = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var ray = new Ray(
    origin: new PointXY(-10f, 0f),
    angle: 0f);

List<PointXY> intersections = boundary.GetPointIntersections(ray);

// intersections contains (-5, 0) and (5, 0).
```

The angle passed to `Ray` is measured in radians. `new Ray(origin)` is a shorter way to create a
ray pointing along the positive X axis.

The result is empty when the curve is missed or lies entirely behind the ray. A tangent usually
produces one point. A ray that starts inside a closed contour returns only the forward exit.
Ray results are ordered in the forward direction of the ray.

The returned `List<PointXY>` is new, mutable, and owned by the caller. It can be sorted, filtered,
or reused without changing the curve.

## Keep a supported concrete type

Ray intersection is a binary operation exposed by extension methods, not a curve-interface
capability. Keep a supported concrete geometry type so the compiler can bind its overload.
Supported non-composite contour types also provide concrete overloads. `CompositeContour` and
`ParameterizedCompositeContour` do not: their heterogeneous components are retained through
`IContourPath`, which has no common binary-intersection operation.

Linear and circular curves (`Line`, `Ray`, `Segment`, `ParameterizedLine`,
`ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc`, and `ParameterizedArc`), plus
`QuadraticBezier` and `CubicBezier`, provide concrete extension overloads for `Ray`, `Line`,
`ParameterizedLine`, `Segment`, and `ParameterizedSegment`:

```csharp
var segment = new Segment(new PointXY(-2f, 1f), new PointXY(2f, 1f));
var probeLine = new Line(new PointXY(0f, -2f), new PointXY(0f, 2f));

List<PointXY> lineIntersections = segment.GetPointIntersections(probeLine);
```

Multiple intersections are ordered along the canonical direction of a `Line` or the
parameterized direction of a `ParameterizedLine`. A `Segment` orders them from `EndpointA` to
`EndpointB`, while a `ParameterizedSegment` orders them from `StartPoint` to `EndPoint`. Both
segment types restrict results according to endpoint inclusion.

Spatial2D does not define one general curve-versus-curve intersection method.

Ray and line intersections for `QuadraticBezier` and `CubicBezier` solve the polynomial of the
original curve rather than intersecting its internal polyline approximation.

Ray intersections and intersections with `Line`, `ParameterizedLine`, `Segment`, or
`ParameterizedSegment` do not accept or apply a geometry-epsilon parameter. Linear cases use
exact comparisons, while polynomial cases isolate roots in `double` and return `float`
coordinates.

## Account for overlaps and endpoints

`GetPointIntersections` reports only isolated points. An overlap can represent infinitely many
points, so points that belong to that continuous set are omitted. An isolated meeting at an
included segment endpoint is returned; an excluded endpoint is not.

`ParameterizedSegmentChain` combines the results from its segments and removes exactly equal
shared points. This prevents a ray through a chain vertex from reporting the same location once
for each adjacent segment.

For the underlying geometry model, see [Curves](../../concepts/geometry-model/curves.md). Next,
learn how to [build a closed contour](build-a-closed-contour.md).
