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
float distance = rectangle.Distance(new PointXY(12f, 3f));
float signedDistance = rectangle.SignedDistance(new PointXY(4f, 3f));
```

`distance` is the unsigned distance to the rectangle boundary.
For a rectangle with positive width and height, `signedDistance` is negative inside, zero on the
boundary, and positive outside.

Equal corner coordinates are valid. If one normalized dimension is zero, the rectangle represents
a segment; if both dimensions are zero, it represents a point. A degenerate rectangle has no
interior: `Contains` is true only on the represented segment or point, and `SignedDistance` is zero
there and positive everywhere else. Consequently, `default(Rectangle)` is a valid point rectangle
at the origin.

For raster grids, rectangular bounds are represented with an origin, size, and integer resolution
rather than a region type. `RasterGeometry` retains its separate construction rules; the degenerate
region semantics do not change raster-grid validation.
