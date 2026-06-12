# Build a Closed Contour

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new Contour(new IFinitePath[]
{
    new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(4f, 0f)),
    new ParameterizedSegment(new PointXY(4f, 0f), new PointXY(4f, 4f)),
    new ParameterizedSegment(new PointXY(4f, 4f), new PointXY(0f, 4f)),
    new ParameterizedSegment(new PointXY(0f, 4f), new PointXY(0f, 0f))
});

bool inside = contour.Encloses(new PointXY(2f, 2f));
```
