# Contour-Based Regions

Contour-based regions represent filled areas bounded by one or more closed contours. They support arbitrary boundaries, holes, and nested filled areas.

## [`ContourBasedRegion`](../contour-based-regions.md)

`ContourBasedRegion` applies the even-odd fill rule to its contours. A point belongs to the region when it lies inside an odd number of contours.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;

var outerBoundary = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(6f, 0f),
    new PointXY(6f, 6f),
    new PointXY(0f, 6f));

var holeBoundary = new CompositeContour(
    new PointXY(2f, 2f),
    new PointXY(4f, 2f),
    new PointXY(4f, 4f),
    new PointXY(2f, 4f));

var region = new ContourBasedRegion(new IContour[]
{
    outerBoundary,
    holeBoundary
});

bool contains = region.Contains(new PointXY(1f, 1f));
var contours = region.Contours;
```

The contours are exposed as a read-only structural view. They must be closed and must not intersect or touch each other.
