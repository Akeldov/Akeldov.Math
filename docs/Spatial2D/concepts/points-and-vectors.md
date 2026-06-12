# Points and Vectors

Use points for locations and vectors for movement, size, direction, and component-wise math.

```csharp
using Akeldov.Math.Spatial2D;

var origin = new PointXY(0f, 0f);
var direction = new VectorXY(3f, 4f);

float length = direction.Length;
VectorXY unit = direction.Normalize();
PointXY endpoint = origin + unit * 10f;
```

`PointXY` implements `IHasPosition2D`, so it can be used directly in APIs that work with positioned items.

`VectorXYInt` converts to `VectorXY` implicitly. Convert back explicitly or by using rounding helpers when a floating-point vector must become an integer grid coordinate.

```csharp
var cell = new VectorXYInt(3, 8);

VectorXY asFloat = cell;
VectorXYInt truncated = (VectorXYInt)new VectorXY(3.9f, 8.1f);
VectorXYInt rounded = new VectorXY(3.9f, 8.1f).RoundToInt();
```
