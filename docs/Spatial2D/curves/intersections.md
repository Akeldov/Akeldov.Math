# Intersections

Binary intersection operations are extension methods on supported concrete geometry types. Ray overloads return isolated intersection points in the forward direction of the ray. Curve interfaces, including `ICurve`, `IContourPath`, and `IContour`, do not declare ray intersections; keep a supported concrete type when calling an intersection extension. `CompositeContour` and `ParameterizedCompositeContour` do not provide ray-intersection overloads because their heterogeneous paths have no common binary-intersection contract.

Linear, circular, Bezier, B-spline, and NURBS curves provide binary intersection overloads for all supported concrete curve types. This includes `BSpline`–`BSpline`, `BSpline`–`Nurbs`, and `Nurbs`–`Nurbs` intersections in both call directions.

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
`EndPoint`. A target `QuadraticBezier`, `CubicBezier`, `BSpline`, or `Nurbs` orders intersections
from its `StartPoint` to its `EndPoint`. Both segment types respect endpoint inclusion.

Points that belong to a continuous set of intersections are not returned. For example, a
collinear overlap between a linear curve and the ray does not produce a representative point.

Ray intersections and intersections with `Line`, `ParameterizedLine`, `Segment`, `ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc`, or `ParameterizedArc` do not accept or apply a geometry-epsilon parameter. Linear cases use exact comparisons;
`QuadraticBezier` and `CubicBezier` solve the polynomial of the original curve rather than
intersecting a polyline approximation. Their `Arc` and `ParameterizedArc` overloads isolate quartic
or sextic roots in `double` and round the resulting coordinates to `float`, without a
geometry-epsilon parameter. Bezier-target intersections similarly isolate the original resultant
of degree up to nine without flattening either curve. B-spline and NURBS intersections convert
each non-empty knot span to its original polynomial or homogeneous rational form and isolate the
resulting roots or resultants in `double`; they never use `Flatten` or `SegmentsPerKnotSpan`.
