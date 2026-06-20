# Collection Ownership

Collection return types communicate ownership.

Mutable collections such as `List<T>` and arrays are used for newly computed transient results that the library does not retain. Callers may filter, append to, or reuse those returned collections.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(new PointXY(0f, 0f), 5f);
var ray = new Ray(new PointXY(-10f, 0f));

List<PointXY> hits = circle.GetRayIntersections(ray);
hits.RemoveAll(point => point.X < 0f);
```

`IReadOnlyList<T>` is used for structural views, retained state, validated pass-through inputs, or semantic algorithm results whose order, cardinality, or invariants should not be changed through the public contract.

Voronoi partitions and contour curve lists are examples where `IReadOnlyList<T>` helps preserve the meaning of the result.
