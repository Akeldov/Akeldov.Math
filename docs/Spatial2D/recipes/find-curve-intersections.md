# Find Curve Intersections

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(new PointXY(0f, 0f), 5f);
var ray = new Ray(new PointXY(-10f, 0f), angle: 0f);

List<PointXY> intersections = circle.GetRayIntersections(ray);
```

The returned list is new, mutable, and owned by the caller.
