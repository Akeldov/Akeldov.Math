# Curves

Curves describe one-dimensional geometry in two-dimensional space. Spatial2D includes infinite
lines, rays, finite segments, circular arcs, and Bezier paths. They form the edges of
[contours](contours.md), provide distance and projection queries, and can act as sources for
fields and rasterizers.

Most curve types live in the <xref:Akeldov.Math.Spatial2D.Curves> namespace. Full circles and
other closed boundaries are contours, even though they support the common curve operations.

## Understand the curve model

The curve interfaces add capabilities instead of imposing one representation:

| Interface | Capability |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Curves.ICurve> | Measures point distance and projects points. |
| <xref:Akeldov.Math.Spatial2D.Curves.IFiniteCurve> | Exposes a finite `Length` in world coordinate units. |
| <xref:Akeldov.Math.Spatial2D.Curves.IOneEndpointCurve> | Has one endpoint, as a ray does. |
| <xref:Akeldov.Math.Spatial2D.Curves.ITwoEndpointCurve> | Has two endpoints without defining a traversal order. |
| <xref:Akeldov.Math.Spatial2D.Curves.IParameterizedCurve> | Maps a curve coordinate to a point and reports that coordinate during projection. |
| <xref:Akeldov.Math.Spatial2D.Curves.IPath> | Adds ordered `StartPoint` and `EndPoint` properties to a parameterized curve. |
| <xref:Akeldov.Math.Spatial2D.Curves.IFinitePath> | Combines a finite length, ordered endpoints, and parameterization. |
| <xref:Akeldov.Math.Spatial2D.Curves.IRightwardCrossingProvider> | Counts fill-rule crossings of a horizontal rightward ray. |
| <xref:Akeldov.Math.Spatial2D.Curves.IContourPath> | Combines `IFinitePath` with fill-rule crossings for contour construction and enclosure. |

Use the narrowest interface that expresses the operation. For example, an algorithm that only
needs proximity can accept `ICurve`, an algorithm that walks from one endpoint to another can
accept `IFinitePath`, and a composite-contour builder should accept `IContourPath`.

## Choose a concrete curve

Choose a type from the curve's extent and whether traversal direction matters:

| Family | Type | Extent and curve-coordinate domain | Use when |
|---|---|---|---|
| Linear | <xref:Akeldov.Math.Spatial2D.Curves.Line> | Infinite; not parameterized | Only the geometric line matters. |
| Linear | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedLine> | Infinite; `(-infinity, +infinity)` | An origin, direction, and signed coordinate along the line are required. |
| Linear | <xref:Akeldov.Math.Spatial2D.Curves.Ray> | Half-infinite; `[0, +infinity)` | Geometry behind the origin must be excluded. |
| Linear | <xref:Akeldov.Math.Spatial2D.Curves.Segment> | Finite; not parameterized | Endpoint order should not affect geometric identity. |
| Linear | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedSegment> | Finite; `[0, Length]` | Direction or distance from the start point matters. |
| Linear | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedSegmentChain> | Finite open polyline; `[0, Length]` | Consecutive segments should behave as one path. |
| Circular | <xref:Akeldov.Math.Spatial2D.Curves.Arc> | Finite angular span; not parameterized | Only the arc geometry matters. |
| Circular | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedArc> | Directed angular span; `[0, Length]` | Clockwise or counterclockwise traversal matters. |
| Bezier | <xref:Akeldov.Math.Spatial2D.Curves.QuadraticBezier> | Finite path; `[0, Length]` | One control point is sufficient. |
| Bezier | <xref:Akeldov.Math.Spatial2D.Curves.CubicBezier> | Finite path; `[0, Length]` | Separate outgoing and incoming control points are needed. |

<xref:Akeldov.Math.Spatial2D.Contours.Circle> represents a complete circular boundary. Use
<xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCircle> when a full circle also needs a
traversal origin and direction.

Angles passed to lines, rays, and arcs are expressed in radians. See
[Angles and Units](../fundamentals/angles-and-units.md) for the coordinate and rotation
conventions.

## Work with curve coordinates

Curve coordinates are distances measured in world coordinate units; they are not normalized to
`[0, 1]`. A finite path starts at coordinate `0` and ends at `Length`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

PointXY halfway = path.GetPoint(path.Length * 0.5f); // (5, 0)
```

Infinite parameterized lines use signed coordinates, and rays accept non-negative coordinates.
The direction stored by a path determines which endpoint has coordinate zero.

Bezier curves also expose `GetPointAt(t)`, where `t` is the normalized Bezier parameter in
`[0, 1]`. This parameter is not generally proportional to arc length. Use `GetPoint` when the
coordinate must represent distance along the approximated path.

## Project points and measure distance

Every `ICurve` can find its closest point to a sample. `Project` returns the projected point and
distance; `ProjectWithParameter` also returns the projected curve coordinate:

```csharp
var sample = new PointXY(4f, 3f);
ParameterizedCurveProjection projection = path.ProjectWithParameter(sample);

PointXY closest = projection.ProjectedPoint;  // (4, 0)
float coordinate = projection.CurveCoordinate; // 4
float distance = projection.Distance;          // 3
```

`Distance(point)` is the shorter choice when the closest position and curve coordinate are not
needed. Linear and circular types calculate these operations analytically. Bezier types use an
internal polyline approximation for length, projection, and distance; their intersections solve
the original curve polynomial.

## Intersect a curve with a ray

Concrete `GetPointIntersections` extension methods return isolated intersection points in the
forward direction of the supplied ray. Curve interfaces do not declare this binary operation.
Supported non-composite contour types expose it through extension methods; `CompositeContour`
and `ParameterizedCompositeContour` do not provide ray-intersection overloads:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var ray = new Ray(
    origin: new PointXY(-10f, 0f),
    angle: 0f);

List<PointXY> intersections = circle.GetPointIntersections(ray);
```

The returned list is new, mutable, and owned by the caller. Points that belong to a continuous
set of intersections are not returned. For example, a collinear overlap between a linear curve
and the ray does not produce a representative point.

`IRightwardCrossingProvider.CountRightwardCrossings` is a specialized crossing query used by
containment algorithms. It uses a half-open endpoint rule so shared vertices are not counted
twice.

## Build larger geometry

An `IContourPath` has the ordered endpoints and fill-rule crossing query required to join curves
into a contour. Consecutive paths must meet end-to-start, and a closed contour must connect its
final endpoint back to its first endpoint. The contour can then define the outer boundary or a
hole of a [region](regions.md).

For practical examples, see:

- [Project a point onto a curve](../../how-to-guides/curves-and-contours/project-a-point-onto-a-curve.md)
- [Find curve intersections](../../how-to-guides/curves-and-contours/find-curve-intersections.md)
- [Build a closed contour](../../how-to-guides/curves-and-contours/build-a-closed-contour.md)
- [Curves and transformations tutorial](../../tutorials/2d-geometry-fundamentals/curves-and-transformations.md)
