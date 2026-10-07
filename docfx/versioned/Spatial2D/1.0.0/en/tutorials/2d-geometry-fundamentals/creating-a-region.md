# Creating a Region

In this part of the tutorial, you will turn the closed contour from the previous step into a
filled region and use it to classify points. Continue in the `Spatial2D.Fundamentals` project
with `contour` and the `ToWorld` function already defined in `Program.cs`.

## Fill the contour

Add the regions namespace at the top of `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D.Regions;
```

Create a <xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion> from the contour:

```csharp
var region = new ContourBasedRegion(
    new IContour[] { contour },
    FillRule.EvenOdd);

Console.WriteLine($"Contour count: {region.Contours.Count}");
Console.WriteLine($"Fill rule:     {region.FillRule}");
```

The output is:

```text
Contour count: 1
Fill rule:     EvenOdd
```

A contour describes only a closed boundary. `ContourBasedRegion` interprets that boundary as a
filled two-dimensional area. The even-odd rule fills points enclosed by an odd number of
contours; with this single contour, it fills its interior.

`Contours` is a read-only structural view. The region copies the contour references into its own
storage, so callers cannot change the number or order of its boundaries through the public
contract.

## Classify points

Choose one point inside the shape and one to its left. Define them in the same local coordinate
system as the original boundary, then transform them into world space:

```csharp
PointXY insidePoint = ToWorld(new PointXY(3f, 2.5f));
PointXY outsidePoint = ToWorld(new PointXY(0f, 2.5f));

bool containsInsidePoint = region.Contains(insidePoint);
bool containsOutsidePoint = region.Contains(outsidePoint);

Console.WriteLine($"Inside point:  {containsInsidePoint}");
Console.WriteLine($"Outside point: {containsOutsidePoint}");
```

This prints:

```text
Inside point:  True
Outside point: False
```

Transforming the sample points with the same `ToWorld` function keeps them aligned with the
scaled, rotated, and translated boundary. Passing the untransformed local points to `Contains`
would test different world positions.

`Contains` returns `true` for points inside the filled area and for points on its boundary.

## Measure signed distance

Use `SignedDistance` when an algorithm needs both proximity to the boundary and the side on
which the point lies:

```csharp
float insideDistance = region.SignedDistance(insidePoint);
float outsideDistance = region.SignedDistance(outsidePoint);
float boundaryDistance = region.Distance(worldEdge.StartPoint);

Console.WriteLine($"Inside distance is negative:  {insideDistance < 0f}");
Console.WriteLine($"Outside distance is positive: {outsideDistance > 0f}");
Console.WriteLine($"Boundary distance:            {boundaryDistance}");
```

The result is:

```text
Inside distance is negative:  True
Outside distance is positive: True
Boundary distance:            0
```

Signed distance is negative inside the region, zero on the boundary, and positive outside.
`Distance` always returns a non-negative distance to the nearest boundary and is useful when the
inside/outside distinction does not matter.

You now have a filled shape that can participate in containment, distance, and rasterization
algorithms. Continue with
[Checking Points and Intersections](checking-points-and-intersections.md).
