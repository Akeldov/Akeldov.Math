# Rectangle

`Rectangle` is an axis-aligned filled region.

Use it for simple bounds checks, sampling areas, and rectangular region inputs.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var rectangle = new Rectangle(
    cornerA: new PointXY(0f, 0f),
    cornerB: new PointXY(10f, 6f));

bool contains = rectangle.Contains(new PointXY(4f, 3f));
```

For raster grids, rectangular bounds are represented with an origin, size, and integer resolution rather than a region type.
