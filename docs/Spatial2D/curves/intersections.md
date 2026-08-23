# Intersections

Binary intersection operations are extension methods on concrete geometry types. Ray overloads return isolated intersection points in the forward direction of the ray. `IContourPath` declares its ray overload directly so composite contours can query heterogeneous paths polymorphically. `IContour` does not declare ray intersections; keep a concrete contour type when calling its intersection extension. The base `ICurve` contract is limited to point distance and projection.

Linear and circular curves provide overloads for intersections with `Ray`, `Line`, `ParameterizedLine`, `Segment`, `ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc`, and `ParameterizedArc`. `QuadraticBezier` and `CubicBezier` provide the ray and linear target overloads and numerical `Arc` and `ParameterizedArc` overloads. All of these source types also provide overloads for target `QuadraticBezier` and `CubicBezier` curves.

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
`EndPoint`. A target `QuadraticBezier` or `CubicBezier` orders intersections from its `StartPoint`
to its `EndPoint`. Both segment types respect endpoint inclusion.

Points that belong to a continuous set of intersections are not returned. For example, a
collinear overlap between a linear curve and the ray does not produce a representative point.

Ray intersections and intersections with `Line`, `ParameterizedLine`, `Segment`, `ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc`, or `ParameterizedArc` do not accept or apply a geometry-epsilon parameter. Linear cases use exact comparisons;
`QuadraticBezier` and `CubicBezier` solve the polynomial of the original curve rather than
intersecting a polyline approximation. Their `Arc` and `ParameterizedArc` overloads isolate quartic
or sextic roots in `double` and round the resulting coordinates to `float`, without a
geometry-epsilon parameter. Bezier-target intersections similarly isolate the original resultant
of degree up to nine without flattening either curve.
