# Create a Region with Holes

Use multiple contours with the even-odd fill rule.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;

var region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});

bool insideOuterArea = region.Contains(new PointXY(0.5f, 0.5f));
bool insideHole = region.Contains(new PointXY(2f, 2f));

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
