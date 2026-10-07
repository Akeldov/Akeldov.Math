# Building a Closed Contour

In this part of the tutorial, you will close the two-path chain from the previous step and turn it
into a contour. Continue in the `Spatial2D.Fundamentals` project with the transformed
`worldEdge` and `worldSide` paths already in `Program.cs`.

## Add the missing paths

The existing chain starts at the transformed `start` point and finishes at the transformed `end`
point. Add an upper-left vertex in local space, transform it with the same `ToWorld` function,
and connect the chain back to its start:

```csharp
var upperLeft = new PointXY(1f, 4f);
PointXY worldUpperLeft = ToWorld(upperLeft);

var worldTopEdge = new ParameterizedSegment(
    worldSide.EndPoint,
    worldUpperLeft);

var worldLeftEdge = new ParameterizedSegment(
    worldUpperLeft,
    worldEdge.StartPoint);
```

The four paths now connect in this order:

```text
worldEdge -> worldSide -> worldTopEdge -> worldLeftEdge -> worldEdge
```

Each path's `EndPoint` is the next path's `StartPoint`. The last segment ends at the first path's
start, so the chain is closed.

## Construct the contour

Add the contours namespace at the top of `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D.Contours;
```

Replace the `openBoundary` array from the previous step with a closed boundary and pass it to
<xref:Akeldov.Math.Spatial2D.Contours.CompositeContour>:

```csharp
IFinitePath[] closedBoundary =
{
    worldEdge,
    worldSide,
    worldTopEdge,
    worldLeftEdge
};

var contour = new CompositeContour(closedBoundary);

Console.WriteLine($"Path count:      {contour.Curves.Count}");
Console.WriteLine($"Boundary length: {contour.Length}");
```

`Curves` preserves the traversal order as a read-only structural view. `Length` is the sum of the
four path lengths in world units; the Bezier contribution is based on the library's curve
approximation.

The constructor validates the entire chain immediately. It throws `ArgumentException` when paths
are missing, disconnected, or in the wrong order. Connections use Spatial2D's default geometry
tolerance, so endpoints produced by ordinary floating-point calculations may still form a valid
chain.

## Verify the closing connection

The contour constructor has already guaranteed closure, but you can inspect the connection while
learning the API:

```csharp
IFinitePath firstPath = contour.Curves[0];
IFinitePath lastPath = contour.Curves[contour.Curves.Count - 1];

bool closes = lastPath.EndPoint.AlmostEquals(firstPath.StartPoint);
Console.WriteLine($"Closed:          {closes}");
```

The final line is:

```text
Closed:          True
```

Do not add an extra zero-length segment after `worldLeftEdge`. A closed chain needs matching first
and last positions, not a duplicate path.

## Use a continuous contour coordinate when needed

`CompositeContour` represents the boundary without choosing a global traversal coordinate. When
you need to walk continuously across all four paths, construct a
<xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCompositeContour> from the same array:

```csharp
var parameterizedContour = new ParameterizedCompositeContour(closedBoundary);

PointXY halfwayAround = parameterizedContour.GetPoint(
    parameterizedContour.Length * 0.5f);

Console.WriteLine($"Halfway around:   {halfwayAround}");
```

Its coordinate starts at `worldEdge.StartPoint`, advances through the paths in array order, and
runs from `0` through the total `Length`. Because the contour is closed, coordinates `0` and
`Length` identify the same position.

## Create a polygon directly from vertices

When every edge should be straight, use the point constructor instead. It joins consecutive
vertices with parameterized segments and automatically closes the last vertex back to the first:

```csharp
var polygon = new CompositeContour(
    ToWorld(start),
    ToWorld(corner),
    ToWorld(end),
    ToWorld(upperLeft));
```

Do not use this shortcut for the tutorial's mixed boundary: it would replace `worldSide` with a
straight segment and discard the Bezier shape.

You now have a validated closed boundary. Continue with
[Creating a Region](creating-a-region.md).
