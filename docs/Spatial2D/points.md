# Points

`PointXY` represents a two-dimensional position with single-precision floating-point coordinates.

```csharp
using Akeldov.Math.Spatial2D;

var point = new PointXY(3.5f, 8f);
var offset = new VectorXY(1f, 2f);

PointXY moved = point + offset;
VectorXY displacement = moved - point;
```

`PointXY` implements `IHasPosition2D`, so it can be used directly with positioned APIs such as weighted Voronoi partitioning.

```csharp
IHasPosition2D positioned = new PointXY(2f, 4f);
PointXY position = positioned.Position;
```

Point extension methods cover distance, interpolation, and tolerance-based comparison.

```csharp
var a = new PointXY(0f, 0f);
var b = new PointXY(10f, 0f);

float distance = a.Distance(b);
PointXY middle = a.Lerp(b, 0.5f);
```
