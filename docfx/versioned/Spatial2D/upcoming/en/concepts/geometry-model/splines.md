# B-splines and NURBS

Spatial2D provides two finite directed spline paths:

| Type | Shape data | Use when |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Curves.BSpline> | Degree, control points, and a nondecreasing knot vector | A polynomial B-spline is sufficient. |
| <xref:Akeldov.Math.Spatial2D.Curves.Nurbs> | The same data plus one positive weight per control point | Rational weights must pull the curve toward selected control points, or an exact rational shape such as a circular arc is required. |

Both types implement <xref:Akeldov.Math.Spatial2D.Curves.IContourPath>. They provide ordered
endpoints, length-based traversal, point projection, distance queries, and the crossing query
needed by composite contours.

## Create a B-spline

The knot count must equal the control-point count plus the degree plus one. This clamped cubic
example repeats each domain endpoint `degree + 1` times, so the curve starts at the first control
point and ends at the last:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var curve = new BSpline(
    degree: 3,
    controlPoints: new[]
    {
        new PointXY(0f, 0f),
        new PointXY(1f, 3f),
        new PointXY(3f, -1f),
        new PointXY(4f, 2f)
    },
    knots: new[] { 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f });

PointXY middle = curve.GetPointAt(0.5f);
```

An unclamped knot vector is also valid. The active knot domain is
`[knots[degree], knots[controlPoints.Count]]`; in that case `StartPoint` and `EndPoint` need not
equal the first and last control points.

Knots must be finite and nondecreasing, and the active domain must have positive width. Interior
knot multiplicity cannot exceed the degree because the path must remain continuous. Endpoint and
out-of-domain multiplicity cannot exceed `degree + 1`.

## Add rational weights

`Nurbs` adds one finite, strictly positive weight per control point. Equal weights produce the
same shape as `BSpline`; changing a weight changes how strongly its control point influences the
curve:

```csharp
float diagonalWeight = MathF.Sqrt(0.5f);

var quarterCircle = new Nurbs(
    degree: 2,
    controlPoints: new[]
    {
        new PointXY(1f, 0f),
        new PointXY(1f, 1f),
        new PointXY(0f, 1f)
    },
    weights: new[] { 1f, diagonalWeight, 1f },
    knots: new[] { 0f, 0f, 0f, 1f, 1f, 1f });

PointXY pointOnArc = quarterCircle.GetPointAt(0.5f);
```

This rational quadratic represents an exact quarter circle when evaluated with `GetPointAt` or
`GetPointAtKnot`.

## Choose the parameter you need

Spline paths expose three related coordinate systems:

| Member | Input domain | Meaning |
|---|---|---|
| `GetPointAt(t)` | `[0, 1]` | Maps a normalized value linearly onto the active knot domain and evaluates the spline. |
| `GetPointAtKnot(knot)` | `[KnotStart, KnotEnd]` | Evaluates the spline in the original knot units. |
| `GetPoint(curveCoordinate)` | `[0, Length]` | Walks by approximate distance from `StartPoint`, measured in world coordinate units. |

The normalized and knot parameters are generally not proportional to arc length. Use `GetPoint`
when movement or placement must be based on distance along the curve.

## Control approximation quality

`GetPointAt` and `GetPointAtKnot` evaluate the polynomial or rational spline directly with de
Boor's algorithm. `Length`, `GetPoint`, `Distance`, `Project`, `ProjectWithParameter`,
`CountRightwardCrossings`, and `Flatten` use a cached polyline approximation.

The constructor's `segmentsPerKnotSpan` argument controls the number of equal parameter
subdivisions in every non-empty knot span. Its default is `64`. Increase it for sharp bends,
highly uneven spans, or extreme NURBS weights. It controls resolution rather than a geometric
error bound.

```csharp
var detailed = new BSpline(
    degree: 3,
    controlPoints: controlPoints,
    knots: knots,
    segmentsPerKnotSpan: 128);

ParameterizedCurveProjection projection =
    detailed.ProjectWithParameter(new PointXY(2f, 1f));

float distance = projection.Distance;
float distanceFromStart = projection.CurveCoordinate;
```

`Flatten` returns a new mutable list of directed segments owned by the caller. `ControlPoints`,
`Knots`, and `Nurbs.Weights` are read-only views of state copied during construction, so later
changes to the input collections do not change the curve.

See [Curves](curves.md) for the shared curve contracts, or
[build a closed contour](../../how-to-guides/curves-and-contours/build-a-closed-contour.md) to use
spline paths as boundary pieces.
