# Collection Ownership

Collection return types communicate one of two ownership contracts used throughout the library:

- **Caller Ownership** grants the caller full ownership of a new mutable collection.
- **Library Ownership** preserves library state or semantic invariants by exposing a read-only collection contract.

## Caller Ownership

Mutable collections such as `List<T>` and arrays are used for newly computed transient results that the library does not retain. Callers may filter, append to, or reuse those returned collections.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(new PointXY(0f, 0f), 5f);
var ray = new Ray(new PointXY(-10f, 0f));

List<PointXY> hits = circle.GetPointIntersections(ray);
hits.RemoveAll(point => point.X < 0f);
```

## Library Ownership

Library Ownership means the library keeps control over mutation through its public contract. The caller receives an `IReadOnlyList<T>` and may inspect, enumerate, or index its elements, but should not add, remove, replace, or reorder them.

This contract is used for structural views, retained state, validated pass-through inputs, and semantic algorithm results whose order, cardinality, or other invariants must be preserved.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var contour = new CompositeContour(
    new IFinitePath[]
    {
        new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f)),
        new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(0f, 2f)),
        new ParameterizedSegment(new PointXY(0f, 2f), new PointXY(0f, 0f))
    });

IReadOnlyList<IFinitePath> curves = contour.Curves;
```

Library Ownership describes the returned API surface rather than necessarily identifying who allocated the underlying collection. For example, a validated input may be returned as-is without granting mutation through the returned contract.

Voronoi partitions and contour curve lists are examples where `IReadOnlyList<T>` helps preserve the meaning of the result.
