# Create a B-spline

Use <xref:Akeldov.Math.Spatial2D.Curves.BSpline> to build a finite directed polynomial spline from
a degree, control points, and a nondecreasing knot vector. The resulting curve implements
<xref:Akeldov.Math.Spatial2D.Curves.IContourPath>, so it can be traversed, projected onto, and used
as a piece of a composite contour.

## Define the control points and knots

The knot count must equal the control-point count plus the degree plus one. This clamped cubic
example repeats each domain endpoint `degree + 1` times, so the curve starts at the first control
point and ends at the last:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

PointXY[] controlPoints =
{
    new PointXY(0f, 0f),
    new PointXY(1f, 3f),
    new PointXY(3f, -1f),
    new PointXY(4f, 2f)
};

float[] knots = { 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f };

var curve = new BSpline(
    degree: 3,
    controlPoints: controlPoints,
    knots: knots);

PointXY middle = curve.GetPointAt(0.5f);
```

An unclamped knot vector is also valid. Its active domain is
`[knots[degree], knots[controlPoints.Count]]`; `StartPoint` and `EndPoint` then need not equal the
first and last control points.

Knots must be finite and nondecreasing, and the active domain must have positive width. Interior
knot multiplicity cannot exceed the degree because the path must remain continuous. Endpoint and
out-of-domain multiplicity cannot exceed `degree + 1`.

## Choose the coordinate system

Use the member whose coordinate matches the operation:

| Member | Input domain | Meaning |
|---|---|---|
| `GetPointAt(t)` | `[0, 1]` | Maps a normalized value linearly onto the active knot domain. |
| `GetPointAtKnot(knot)` | `[KnotStart, KnotEnd]` | Evaluates the spline in the original knot units. |
| `GetPoint(curveCoordinate)` | `[0, Length]` | Walks by approximate distance from `StartPoint` in world units. |

Normalized and knot parameters are generally not proportional to arc length. Use `GetPoint` for
movement or placement based on distance along the curve.

## Tune approximation quality

`GetPointAt` and `GetPointAtKnot` evaluate the polynomial spline directly with de Boor's
algorithm. `Length`, `GetPoint`, `Distance`, `Project`, `ProjectWithParameter`,
`CountRightwardCrossings`, and `Flatten` use a cached polyline approximation.

The optional `segmentsPerKnotSpan` argument controls the number of equal parameter subdivisions
in each non-empty knot span. Its default is `64`. Increase it for sharp bends or highly uneven
spans; it controls resolution rather than a geometric error bound:

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

`Flatten` returns a new mutable list of directed segments owned by the caller. `ControlPoints`
and `Knots` are read-only views of state copied during construction, so later changes to the input
arrays do not change the curve.

To influence the shape with rational weights, [create a NURBS curve](create-a-nurbs-curve.md).
See [Curves](../../concepts/geometry-model/curves.md) for the shared curve contracts, or
[build a closed contour](build-a-closed-contour.md) to use the spline as a boundary piece.
