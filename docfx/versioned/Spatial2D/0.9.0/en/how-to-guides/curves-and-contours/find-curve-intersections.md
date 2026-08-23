# Find Curve Intersections

Use `ICurve.GetPointIntersections` to find the isolated points where a curve or contour meets a
directed ray. Only intersections at the ray origin or in front of it are returned.

## Cast a ray through a curve

The following ray starts to the left of a circle and points along the positive X axis, so it
crosses the boundary twice:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

ICurve boundary = new Circle(
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
Treat the result as an unordered collection unless the concrete curve documents a stronger
ordering guarantee.

The returned `List<PointXY>` is new, mutable, and owned by the caller. It can be sorted, filtered,
or reused without changing the curve.

## Use the common curve interface

The same call works with lines, rays, segments, arcs, Bezier curves, circles, and composite
contours because they implement `ICurve`:

```csharp
static List<PointXY> FindIntersections(ICurve curve, Ray ray)
{
    return curve.GetPointIntersections(ray);
}
```

Linear and circular curves (`Line`, `Ray`, `Segment`, `ParameterizedLine`,
`ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc`, and `ParameterizedArc`), plus
`QuadraticBezier` and `CubicBezier`, also provide `GetPointIntersections` overloads for `Line`,
`ParameterizedLine`, `Segment`, `ParameterizedSegment`, and `ParameterizedSegmentChain`.
Linear and circular curves additionally provide an exact overload for `Arc`:

```csharp
var segment = new Segment(new PointXY(-2f, 1f), new PointXY(2f, 1f));
var probeLine = new Line(new PointXY(0f, -2f), new PointXY(0f, 2f));

List<PointXY> lineIntersections = segment.GetPointIntersections(probeLine);
```

Multiple intersections are ordered along the canonical direction of a `Line` or the
parameterized direction of a `ParameterizedLine`. A `Segment` orders them from `EndpointA` to
`EndpointB`, while a `ParameterizedSegment` orders them from `StartPoint` to `EndPoint`. A
`ParameterizedSegmentChain` orders distinct intersections from its `StartPoint` to its
`EndPoint`. An `Arc` orders intersections counterclockwise from its `StartAngle`. Both segment
types restrict results according to endpoint inclusion.

Spatial2D does not define one general curve-versus-curve intersection method.

Bezier intersections with a ray are calculated against the curve's internal polyline
approximation. Line intersections for `QuadraticBezier` and `CubicBezier` instead solve the
polynomial of the original curve. General `BezierCurve` does not provide these overloads.

`GetPointIntersections(Ray)` uses the library's standard geometry tolerance. Intersections with
`Line`, `ParameterizedLine`, `Segment`, `ParameterizedSegment`, or
`ParameterizedSegmentChain` or `Arc` use exact comparisons. Bezier curves do not provide the
`Arc` overload because a general cubic curve-circle intersection has no exact algebraic solution.

## Account for overlaps and endpoints

`GetPointIntersections` reports only isolated points. An overlap can represent infinitely many
points, so points that belong to that continuous set are omitted. An isolated meeting at an
included segment endpoint is returned; an excluded endpoint is not.

Composite curves and contours combine the results from their component paths and remove shared
points using the library's standard geometry tolerance. This prevents a ray through a contour
vertex from normally reporting the same location once for each adjacent path.

For the underlying geometry model, see [Curves](../../concepts/geometry-model/curves.md). Next,
learn how to [build a closed contour](build-a-closed-contour.md).
