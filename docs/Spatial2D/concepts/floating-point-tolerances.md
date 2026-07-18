# Floating-Point Tolerances

Geometry operations often need tolerance-based comparisons. Use `AlmostEquals` and `GeometryConstants.GeometryEpsilon` when exact component equality would be too strict.

```csharp
using Akeldov.Math.Spatial2D;

var a = new VectorXY(1f, 2f);
var b = new VectorXY(1f + GeometryConstants.GeometryEpsilon / 2f, 2f);

bool closeEnough = a.AlmostEquals(b); // true
```

Many APIs accept a `geometryEpsilon` argument. It is measured in world coordinate units and is used around cases such as tangencies, nearly parallel lines, collinear overlaps, and curve endpoints.
