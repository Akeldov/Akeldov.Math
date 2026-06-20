# Disk

`Disk` is a filled circular region.

Use it for circular bounds checks, circular region inputs, and signed-distance sampling.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var disk = new Disk(
    center: new PointXY(0f, 0f),
    radius: 5f);

bool contains = disk.Contains(new PointXY(3f, 4f));
float distance = disk.Distance(new PointXY(7f, 0f));
float signedDistance = disk.SignedDistance(new PointXY(3f, 0f));
```

`distance` is the unsigned distance to the disk boundary.
`signedDistance` is negative for points inside the disk and positive for points outside it.

Use `ToContour` when the circular boundary is needed as a `Circle`.
