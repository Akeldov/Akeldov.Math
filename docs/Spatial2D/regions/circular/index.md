# Circular Regions

Circular regions represent filled areas defined by a center and a radius.

## [`Disk`](../disk.md)

`Disk` is a filled circular region. It provides point containment and unsigned or signed distance to its circular boundary.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var disk = new Disk(
    center: new PointXY(0f, 0f),
    radius: 5f);

bool contains = disk.Contains(new PointXY(3f, 4f));
float signedDistance = disk.SignedDistance(new PointXY(0f, 0f));
var boundary = disk.ToContour();
```

Use `Disk` when the filled area matters. Use the returned [`Circle`](../../contours/circle.md) when only the boundary is needed.
