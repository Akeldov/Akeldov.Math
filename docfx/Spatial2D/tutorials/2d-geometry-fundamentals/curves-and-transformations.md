# Curves and Transformations

In this part of the tutorial, you will connect points with a straight path and a curved path, then
place that geometry in world space. Continue in the `Spatial2D.Fundamentals` project from the
previous steps.

## Create a directed segment

Replace the contents of `Program.cs` with:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var start = new PointXY(1f, 1f);
var corner = new PointXY(5f, 1f);

var lowerEdge = new ParameterizedSegment(start, corner);
PointXY lowerEdgeMidpoint = lowerEdge.GetPoint(lowerEdge.Length * 0.5f);

Console.WriteLine($"Length:   {lowerEdge.Length}");
Console.WriteLine($"Midpoint: {lowerEdgeMidpoint}");
```

Run the application:

```powershell
dotnet run
```

The output is:

```text
Length:   4
Midpoint: (3, 1)
```

A <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedSegment> has an ordered `StartPoint` and
`EndPoint`. Its curve coordinate is a distance in world units: zero identifies the start, and
`Length` identifies the end. This direction will matter when you join paths into a contour.

Use <xref:Akeldov.Math.Spatial2D.Curves.Segment> instead when endpoint order has no meaning and
you only need the geometric line segment.

## Add a Bezier path

Add a quadratic Bezier path after `lowerEdge`:

```csharp
var end = new PointXY(5f, 4f);
var control = new PointXY(7f, 2.5f);
var roundedSide = new QuadraticBezier(corner, control, end);

PointXY halfwayByParameter = roundedSide.GetPointAt(0.5f);
Console.WriteLine($"Bezier midpoint: {halfwayByParameter}");
```

This prints:

```text
Bezier midpoint: (6, 2.5)
```

The control point pulls the curve toward `(7, 2.5)` without becoming a point on the path itself.
`GetPointAt(t)` uses the normalized Bezier parameter from `0` to `1`. When you need a position at a
distance along the approximated curve, use `GetPoint(curveCoordinate)` with a value from `0` to
`Length` instead.

Both `ParameterizedSegment` and <xref:Akeldov.Math.Spatial2D.Curves.QuadraticBezier> implement
<xref:Akeldov.Math.Spatial2D.Curves.IFinitePath>. This shared interface provides ordered endpoints,
a finite length, and curve-coordinate operations needed by composite contours.

## Translate the paths

Curve types are immutable. Adding a vector creates a translated copy and leaves the original
value unchanged:

```csharp
var translation = new VectorXY(2f, -1f);

ParameterizedSegment movedEdge = lowerEdge + translation;
QuadraticBezier movedSide = roundedSide + translation;

Console.WriteLine($"Moved edge: {movedEdge.StartPoint} -> {movedEdge.EndPoint}");
Console.WriteLine($"Moved side: {movedSide.StartPoint} -> {movedSide.EndPoint}");
```

The two paths remain connected because the same translation is applied to their shared endpoint:

```text
Moved edge: (3, 0) -> (7, 0)
Moved side: (7, 0) -> (7, 3)
```

## Move local geometry into world space

For a scale-and-rotate transformation, transform every defining point and construct new paths.
Add this local function and the transformed paths:

```csharp
PointXY ToWorld(PointXY point) => point.Transform(
    scaleFactor: 1.5f,
    angle: MathF.PI / 6f,
    offset: new VectorXY(10f, 2f));

var worldEdge = new ParameterizedSegment(
    ToWorld(lowerEdge.StartPoint),
    ToWorld(lowerEdge.EndPoint));

var worldSide = new QuadraticBezier(
    ToWorld(roundedSide.StartPoint),
    ToWorld(roundedSide.ControlPoint),
    ToWorld(roundedSide.EndPoint));
```

`Transform` applies these operations in order:

1. Uniformly scale relative to the origin.
2. Rotate counterclockwise around the origin by `PI / 6` radians (30 degrees).
3. Add the translation offset.

Applying one function to all defining points preserves the connection between the two paths. If
you need to rotate around a particular point instead, use `point.Rotate(pivot, angle)`; its angle
is also expressed in radians.

## Keep the paths in traversal order

Store the transformed paths behind their common interface, in the order in which they should be
traversed:

```csharp
IFinitePath[] openBoundary =
{
    worldEdge,
    worldSide
};

foreach (IFinitePath path in openBoundary)
    Console.WriteLine($"{path.StartPoint} -> {path.EndPoint}");
```

The boundary is still open: `worldSide.EndPoint` does not connect back to
`worldEdge.StartPoint`. In the next step, you will add the missing paths and validate that the
chain closes.

Continue with [Building a Closed Contour](building-a-closed-contour.md).
