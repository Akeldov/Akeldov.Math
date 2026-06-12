# Scaling

The library uses `IScalable<T>` for types that can produce scaled copies of themselves.

List scaling helpers return newly computed caller-owned collections. This is useful when the original data describes a model space and the scaled copy should be adjusted for another coordinate system.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

IReadOnlyList<MyScalableItem> source = LoadItems();
List<MyScalableItem> scaled = source.Scale(new VectorXY(2f, 2f));
```

Scaling does not mutate the source collection.
