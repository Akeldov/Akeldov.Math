# Composite Contours

Composite contours form a closed boundary from consecutive `IContourPath` instances. They support mixed line, arc, Bezier, and other contour-path segments as long as every path continues from the previous endpoint and the final path closes the chain.

## Choosing a Type

| Type | Curve coordinate | Use when |
| --- | --- | --- |
| [`CompositeContour`](../composite-contour.md) | None | The closed boundary and its geometric operations are sufficient. |
| [`ParameterizedCompositeContour`](../parameterized-composite-contour.md) | `[0, Length]` | The complete chain needs one cumulative traversal coordinate. |

`CompositeContour` can also connect a point collection with straight segments automatically.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var contour = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(4f, 0f),
    new PointXY(4f, 3f),
    new PointXY(0f, 3f));

float perimeter = contour.Length;                         // 14
bool enclosesCenter = contour.Encloses(new PointXY(2f, 1f)); // true
```

Constructors copy the curve or point collection. `Curves` exposes the retained structure through `IReadOnlyList<IContourPath>`, preserving the order, closure, fill-rule crossings, and direct ray query required from each path.

`ParameterizedCompositeContour` accumulates the lengths of its component paths. `GetPoint`, `ProjectWithParameter`, and other parameterized operations therefore use one coordinate across the entire closed chain rather than restarting at each component.

Corner smoothing with `FilletCorners` returns a new `CompositeContour`; the source contour remains unchanged.
