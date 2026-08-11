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

`GetPointIntersections` is a curve-versus-ray query. Spatial2D does not define one general
curve-versus-curve intersection method. When a full infinite probe line is required, cast two
opposite rays from the same origin and remove a duplicate at their shared origin.

Bezier intersections are calculated against the curve's internal polyline approximation. Use
the result at the same precision as other Bezier length, projection, and distance operations.

## Preserve a custom comparison tolerance in legacy code

`GetPointIntersections(Ray)` uses the library's standard geometry tolerance and is the
recommended API. The obsolete `GetRayIntersections(Ray, float)` overload remains for legacy
code that must choose a custom tolerance. Its `geometryEpsilon` argument is measured in world
coordinate units and controls comparisons near tangencies, endpoints, collinear overlaps, and
nearly parallel curves. For example, a larger tolerance can treat a segment that is very close
to a ray as collinear:

```csharp
const float geometryEpsilon = 0.01f;

ICurve segment = new Segment(
    new PointXY(4f, 0.005f),
    new PointXY(10f, 0.005f));
var ray = new Ray(new PointXY(0f, 0f));

List<PointXY> defaultResult = segment.GetPointIntersections(ray);

#pragma warning disable CS0618 // Legacy API required for a custom tolerance.
List<PointXY> tolerantResult =
    segment.GetRayIntersections(ray, geometryEpsilon);
#pragma warning restore CS0618

// defaultResult is empty.
// tolerantResult contains (4, 0.005).
```

Prefer `GetPointIntersections` unless preserving a custom-tolerance workflow is required. When
the obsolete overload is unavoidable, increase its tolerance only to match the scale and
expected numerical noise of the input. An unnecessarily large value can merge nearby points or
turn a near miss into an intersection. The value must be finite and non-negative.

## Account for overlaps and endpoints

`GetPointIntersections` reports only isolated points. An overlap can represent infinitely many
points, so points that belong to that continuous set are omitted. An isolated meeting at an
included segment endpoint is returned; an excluded endpoint is not.

Composite curves and contours combine the results from their component paths and remove shared
points using the library's standard geometry tolerance. This prevents a ray through a contour
vertex from normally reporting the same location once for each adjacent path.

For the underlying geometry model, see [Curves](../../concepts/geometry-model/curves.md). Next,
learn how to [build a closed contour](build-a-closed-contour.md).
