# Intersections

`ICurve.GetPointIntersections(Ray)` returns isolated intersection points in the forward direction of a ray.
Linear and circular curves provide overloads for intersections with `Line`, `ParameterizedLine`, `Segment`, `ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc`, and `ParameterizedArc`. `QuadraticBezier` and `CubicBezier` provide the linear target overloads and numerical `Arc` and `ParameterizedArc` overloads. All of these source types also provide an overload for a target `QuadraticBezier`.

The returned collection is a new mutable list owned by the caller.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var rayCaster = new Ray(new PointXY(-10f, 0f), angle: 0f);
List<PointXY> hits = circle.GetPointIntersections(rayCaster);
```

```csharp
var segment = new Segment(
    new PointXY(-2f, 1f),
    new PointXY(2f, 1f));

var probeLine = new Line(
    new PointXY(0f, -2f),
    new PointXY(0f, 2f));

List<PointXY> lineHits = segment.GetPointIntersections(probeLine);
```

When a `ParameterizedLine` is supplied, multiple intersections are ordered along its
parameterized direction. A `Line` orders them along its canonical direction, while a `Segment`
orders them from `EndpointA` to `EndpointB`. A `ParameterizedSegment` orders them from
`StartPoint` to `EndPoint`. A `ParameterizedSegmentChain` orders distinct intersections from
the chain's `StartPoint` to its `EndPoint`. An `Arc` orders intersections counterclockwise from
its `StartAngle`, while a `ParameterizedArc` uses its `AngularDirection` from `StartPoint` to
`EndPoint`. A target `QuadraticBezier` orders intersections from its `StartPoint` to its
`EndPoint`. Both segment types respect endpoint inclusion.

Points that belong to a continuous set of intersections are not returned. For example, a
collinear overlap between a linear curve and the ray does not produce a representative point.

`GetPointIntersections(Ray)` uses the library's standard geometry tolerance. Intersections with
`Line`, `ParameterizedLine`, `Segment`, `ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc`, or `ParameterizedArc` use exact comparisons;
`QuadraticBezier` and `CubicBezier` solve the polynomial of the original curve rather than
intersecting a polyline approximation. Their `Arc` and `ParameterizedArc` overloads isolate quartic
or sextic roots in `double` and round the resulting coordinates to `float`, without a
geometry-epsilon parameter. Quadratic-target intersections between Bezier curves similarly
isolate the original quartic or sextic resultant without flattening either curve.
