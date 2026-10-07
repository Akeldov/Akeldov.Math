# Checking Points and Intersections

In this final part of the tutorial, you will query the boundary and filled area built in the
previous steps, project a point onto the contour, and cast a ray through the shape. Continue in
the `Spatial2D.Fundamentals` project with `contour`, `region`, `insidePoint`, `outsidePoint`, and
the `ToWorld` function already defined in `Program.cs`.

## Choose between a contour and a region query

The contour represents one closed boundary, while the region applies a fill rule to one or more
boundaries. Compare their point queries for the tutorial's single-contour shape:

```csharp
bool contourEnclosesInside = contour.Encloses(insidePoint);
bool contourEnclosesOutside = contour.Encloses(outsidePoint);

bool regionContainsInside = region.Contains(insidePoint);
bool regionContainsOutside = region.Contains(outsidePoint);

Console.WriteLine($"Contour encloses inside: {contourEnclosesInside}");
Console.WriteLine($"Contour encloses outside: {contourEnclosesOutside}");
Console.WriteLine($"Region contains inside:   {regionContainsInside}");
Console.WriteLine($"Region contains outside:  {regionContainsOutside}");
```

The output is:

```text
Contour encloses inside: True
Contour encloses outside: False
Region contains inside:   True
Region contains outside:  False
```

For one boundary, <xref:Akeldov.Math.Spatial2D.Contours.IContour.Encloses(Akeldov.Math.Spatial2D.PointXY)>
and <xref:Akeldov.Math.Spatial2D.Regions.IRegion.Contains(Akeldov.Math.Spatial2D.PointXY)> classify
these points the same way, including a point on the boundary. Prefer `Contains` when fill rules,
multiple contours, or holes are part of the model. Prefer `Encloses` when the single boundary is
the primary object.

## Project an outside point onto the boundary

Use `Project` when you need both the closest boundary position and the distance to it:

```csharp
CurveProjection projection = contour.Project(outsidePoint);

Console.WriteLine($"Projected point:     {projection.ProjectedPoint}");
Console.WriteLine($"Projection distance: {projection.Distance}");
Console.WriteLine($"On boundary:         {contour.Distance(projection.ProjectedPoint) == 0f}");
```

`projection.ProjectedPoint` lies on the left edge of this shape, and `projection.Distance` is
approximately `1.5` world units. It agrees with `contour.Distance(outsidePoint)` and
`region.Distance(outsidePoint)` because both objects use the same boundary.

<xref:Akeldov.Math.Spatial2D.Curves.CurveProjection> carries the projected point together with
its non-negative distance. Call `Distance` directly when the closest position itself is not
needed.

## Cast a ray through the contour

Add the generic collections namespace at the top of `Program.cs` if it is not present already:

```csharp
using System.Collections.Generic;
```

Create a ray at `outsidePoint`. Its angle matches the `PI / 6` rotation used by `ToWorld`, so in
the shape's local coordinates the ray travels horizontally from left to right:

```csharp
var probeRay = new Ray(
    origin: outsidePoint,
    angle: MathF.PI / 6f);

List<PointXY> intersections = contour.GetRayIntersections(probeRay);

Console.WriteLine($"Boundary intersections: {intersections.Count}");
```

The ray enters through the straight left edge and exits through the curved right side:

```text
Boundary intersections: 2
```

The angle passed to <xref:Akeldov.Math.Spatial2D.Curves.Ray> is measured in radians. The query
returns only intersections at the ray origin or in front of it; points behind the origin are not
included.

## Select the first intersection

Do not depend on the order returned by a general curve. Sort this caller-owned mutable list by
distance from the ray origin before selecting the entry point:

```csharp
intersections.Sort((left, right) =>
    outsidePoint.Distance(left).CompareTo(outsidePoint.Distance(right)));

PointXY entryPoint = intersections[0];
float distanceToEntry = outsidePoint.Distance(entryPoint);

Console.WriteLine($"Distance to entry: {distanceToEntry}");
Console.WriteLine($"Entry is contained: {region.Contains(entryPoint)}");
```

The entry point is on the boundary, so `region.Contains(entryPoint)` is `true`. Sorting or
filtering `intersections` does not change `contour`: `GetRayIntersections` returns a new mutable
list owned by the caller.

## Handle misses and numerical tolerance

A ray that misses the contour returns an empty list. A tangent usually returns one point, and a
ray that starts inside the shape returns only its forward exit. Always check `Count` before
indexing the result.

The optional `geometryEpsilon` argument controls comparisons near endpoints, tangencies,
collinear overlaps, and nearly parallel curves. It is a finite non-negative distance in world
coordinate units. Start with the default and increase it only when the scale and expected input
noise require a larger tolerance. Composite contours remove duplicate intersections at shared
path endpoints within this tolerance.

Bezier intersections, like Bezier length and projection, use the curve's internal polyline
approximation. Keep that approximation in mind when a query runs very close to the curved side.

## Finish the tutorial

You have now used Spatial2D to:

1. Create points, vectors, straight paths, and a Bezier path.
2. Transform local geometry into world space.
3. Build and validate a closed contour.
4. Turn the contour into a filled region.
5. Classify, project, measure, and intersect points against the resulting geometry.

For focused follow-up examples, see [Project a Point onto a Curve](../../how-to-guides/curves-and-contours/project-a-point-onto-a-curve.md)
and [Find Curve Intersections](../../how-to-guides/curves-and-contours/find-curve-intersections.md).
The [Contours](../../concepts/geometry-model/contours.md) and
[Regions](../../concepts/geometry-model/regions.md) concept pages describe the underlying
contracts in more depth.
