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
```

The rotation is expressed in radians.
