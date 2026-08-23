# Curve Interfaces

Curve interfaces describe which operations and invariants a type exposes.

| Interface | Meaning |
|---|---|
| `ICurve` | Can measure distance and project a point. |
| `IFiniteCurve` | Has finite length. |
| `IOneEndpointCurve` | Has one endpoint, such as a ray origin. |
| `ITwoEndpointCurve` | Has two unordered endpoints. |
| `IParameterizedCurve` | Supports `GetPoint` and `ProjectWithParameter`. |
| `IPath` | A parameterized curve with two endpoints. |
| `IFinitePath` | A finite directed path. |
| `IRayPath` | A half-infinite parameterized path. |
| `IRightwardCrossingProvider` | Can count horizontal rightward crossings for fill rules. |
| `IContourPath` | A finite directed path that can count fill-rule crossings. |

Composite contours require `IContourPath` curves because every boundary segment must be finite, directed, and able to participate in enclosure queries. Binary intersections are exposed as extension methods on supported concrete geometry types; neither `IContourPath` nor `IContour` declares them. Because a composite contour retains heterogeneous paths through `IContourPath`, `CompositeContour` and `ParameterizedCompositeContour` do not provide ray-intersection overloads.
