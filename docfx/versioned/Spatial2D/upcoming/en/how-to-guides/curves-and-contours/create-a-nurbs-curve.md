# Create a NURBS Curve

Use <xref:Akeldov.Math.Spatial2D.Curves.Nurbs> when rational weights must control how strongly
each control point influences a spline. The resulting finite directed curve implements
<xref:Akeldov.Math.Spatial2D.Curves.IContourPath>, so it supports traversal, projection, distance
queries, and use in a composite contour.

## Define the weighted spline

Provide one finite, strictly positive weight for every control point. The knot count must equal
the control-point count plus the degree plus one, following the same knot-vector rules as
<xref:Akeldov.Math.Spatial2D.Curves.BSpline>.

This rational quadratic represents an exact quarter circle when evaluated with `GetPointAt` or
`GetPointAtKnot`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

float diagonalWeight = MathF.Sqrt(0.5f);

PointXY[] controlPoints =
{
    new PointXY(1f, 0f),
    new PointXY(1f, 1f),
    new PointXY(0f, 1f)
};

float[] weights = { 1f, diagonalWeight, 1f };
float[] knots = { 0f, 0f, 0f, 1f, 1f, 1f };

var quarterCircle = new Nurbs(
    degree: 2,
    controlPoints: controlPoints,
    weights: weights,
    knots: knots);

PointXY pointOnArc = quarterCircle.GetPointAt(0.5f);
```

Equal weights produce the same shape as a B-spline built from the same degree, control points,
and knots. Increase a weight to pull the curve closer to its corresponding control point.

## Choose the coordinate system

Use `GetPointAt(t)` with a normalized parameter in `[0, 1]`, or `GetPointAtKnot(knot)` with a
value in `[KnotStart, KnotEnd]`, to evaluate the rational spline directly. Use
`GetPoint(curveCoordinate)` with a coordinate in `[0, Length]` when movement or placement must be
based on approximate distance from `StartPoint`.

Normalized and knot parameters are generally not proportional to arc length. The
[B-spline guide](create-a-b-spline.md#choose-the-coordinate-system) compares the three coordinate
systems in a table.

## Tune approximation quality

Direct `GetPointAt` and `GetPointAtKnot` evaluation uses de Boor's algorithm and preserves the
exact rational form. `Length`, `GetPoint`, `Distance`, `Project`, `ProjectWithParameter`,
`CountRightwardCrossings`, and `Flatten` use a cached polyline approximation.

Set `segmentsPerKnotSpan` when those approximate operations need more subdivisions. Its default
is `64`; increase it for sharp bends, highly uneven spans, or extreme weights:

```csharp
var detailedQuarterCircle = new Nurbs(
    degree: 2,
    controlPoints: controlPoints,
    weights: weights,
    knots: knots,
    segmentsPerKnotSpan: 128);

ParameterizedCurveProjection projection =
    detailedQuarterCircle.ProjectWithParameter(new PointXY(0.7f, 0.7f));

float distance = projection.Distance;
float distanceFromStart = projection.CurveCoordinate;
```

The subdivision count controls resolution rather than a geometric error bound. `Flatten` returns
a new mutable list of directed segments owned by the caller. `ControlPoints`, `Weights`, and
`Knots` are read-only views of state copied during construction, so later changes to the input
arrays do not change the curve.

See [Create a B-spline](create-a-b-spline.md) for knot multiplicity rules,
[Curves](../../concepts/geometry-model/curves.md) for the shared contracts, or
[Build a closed contour](build-a-closed-contour.md) to use the NURBS curve as a boundary piece.
