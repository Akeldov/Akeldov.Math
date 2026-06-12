# Contour-Based Regions

`ContourBasedRegion` fills one or more closed contours.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;

var region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f)
});

bool isInside = region.Contains(new PointXY(2f, 2f));

static Contour CreateSquareContour(float left, float bottom, float right, float top)
{
    return new Contour(new IFinitePath[]
    {
        new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
        new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
        new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
        new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
    });
}
```

Contour-based region contours must not intersect or touch each other.

## Holes and Nested Contours

Contour-based regions currently use `FillRule.EvenOdd`.
A point is inside the region when it lies inside an odd number of contours.

This supports holes and nested contours.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;

var regionWithHole = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});

bool isInsideOuterArea = regionWithHole.Contains(new PointXY(0.5f, 0.5f));
bool isInsideHole = regionWithHole.Contains(new PointXY(2f, 2f));
```

`isInsideOuterArea` is `true`. `isInsideHole` is `false`.
