# OrientedRectangle

`OrientedRectangle` represents a filled rectangle that can be rotated in world space.

Use it when area membership should follow a local rectangle frame instead of axis-aligned bounds.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var region = new OrientedRectangle(
    center: new PointXY(0f, 0f),
    size: new VectorXY(8f, 3f),
    rotation: MathF.PI / 6f);

bool contains = region.Contains(new PointXY(2f, 0f));
float distance = region.Distance(new PointXY(3f, 0f));
float signedDistance = region.SignedDistance(new PointXY(0f, 0f));
```

The rotation is expressed in radians. Both size components must be non-negative. If one component
is zero, the region represents a rotated segment; if both are zero, it represents its center point.
`default(OrientedRectangle)` is therefore a valid point rectangle at the origin with zero rotation.

`distance` is the unsigned distance to the oriented rectangle boundary.
For positive width and height, `signedDistance` is negative inside, zero on the boundary, and
positive outside. A degenerate oriented rectangle has no interior: `Contains` is true only on its
segment or point, and `SignedDistance` is zero there and positive everywhere else.
