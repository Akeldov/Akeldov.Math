# Build a Closed Contour

Use `CompositeContour` to turn ordered vertices or a chain of contour paths into one closed
boundary. The contour can then answer enclosure, distance, projection, and ray-intersection
queries.

## Create a polygon from vertices

Pass at least three vertices in boundary order. The constructor connects consecutive points
with parameterized segments and automatically joins the last point back to the first:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(6f, 0f),
    new PointXY(6f, 4f),
    new PointXY(0f, 4f));

float perimeter = contour.Length;                    // 20
bool isInside = contour.Encloses(new PointXY(2f, 1f)); // true
IReadOnlyList<IContourPath> edges = contour.Curves;    // 4 segments
```

Do not repeat the first vertex unless the input format already includes a closing point. Both
forms are accepted, and an explicitly repeated first vertex does not create an extra zero-length
segment.

The constructor preserves the supplied order; it does not sort the vertices. List them while
walking around the boundary, either clockwise or counterclockwise. Orientation does not change
basic enclosure queries, but it determines traversal direction when the paths are later used in
a parameterized contour.

## Combine segments, arcs, and Bezier paths

For a mixed boundary, build an `IReadOnlyList<IContourPath>`. Each path's `EndPoint` must meet the
next path's `StartPoint`, and the last path must reconnect to the first. This example joins a
diameter to a counterclockwise semicircle:

```csharp
using System;

IContourPath[] paths =
{
    new ParameterizedSegment(
        startPoint: new PointXY(-2f, 0f),
        endPoint: new PointXY(2f, 0f)),
    new ParameterizedArc(
        center: new PointXY(0f, 0f),
        radius: 2f,
        startAngle: 0f,
        endAngle: MathF.PI,
        angularDirection: AngularDirection.Counterclockwise)
};

var curvedContour = new CompositeContour(paths);
```

Arc angles are expressed in radians. `ParameterizedSegment`, `ParameterizedArc`,
`ParameterizedSegmentChain`, and the Bezier path types all implement `IContourPath`. That
interface combines the finite directed path contract with the rightward-crossing and
ray-intersection queries required by composite contours.

The constructor validates connections using the default geometry tolerance, but it does not
reorder paths or move endpoints to close small gaps. Build the paths in traversal order and use
the same point values at shared endpoints when possible.

## Add a coordinate around the boundary

Use `ParameterizedCompositeContour` when code needs to walk around the contour or preserve a
boundary coordinate during projection. Coordinates are distances in world units from the first
path's `StartPoint`:

```csharp
var directedContour = new ParameterizedCompositeContour(contour.Curves);

PointXY start = directedContour.GetPoint(0f);       // (0, 0)
PointXY position = directedContour.GetPoint(5f);    // (5, 0)
PointXY closedEnd =
    directedContour.GetPoint(directedContour.Length); // (0, 0)

ParameterizedCurveProjection projection =
    directedContour.ProjectWithParameter(new PointXY(7f, 1f));

PointXY closest = projection.ProjectedPoint;        // (6, 1)
float coordinate = projection.CurveCoordinate;      // 7
float distance = projection.Distance;               // 1
```

The valid coordinate range is `[0, Length]`. Its two ends identify the same geometric point
because the contour is closed.

## Avoid invalid contour input

Vertex-based construction requires finite coordinates, at least three boundary vertices, and
distinct adjacent vertices. A path-based contour requires at least one non-null `IContourPath`;
every path must have a finite non-negative length, and the total length must remain finite.

Construction checks continuity and closure, but it does not reject every self-intersection or
decide the intended order of an unordered point set. Avoid crossing edges when a simple boundary
with an unambiguous inside is required.

The contour copies the supplied path references into private storage. `Curves` is a read-only
structural view of that copy, so callers cannot add, remove, or reorder its paths through the
public contract.

## Round polygonal corners

Call `FilletCorners` to replace corners between adjacent parameterized segments with tangent
arcs. The radius is measured in world coordinate units:

```csharp
CompositeContour rounded = contour.FilletCorners(radius: 0.5f);
```

The method returns a new contour and leaves the original unchanged. Corners involving other
path types are preserved. Use a finite positive radius appropriate for the neighboring segment
lengths.

For more background, see [Contours](../../concepts/geometry-model/contours.md). You can also
[find ray intersections](find-curve-intersections.md) or use the boundary to
[create a region with holes](../regions/create-a-region-with-holes.md).
