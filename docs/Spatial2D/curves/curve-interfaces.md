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
| `IContourPath` | A finite path that can count fill-rule crossings and report isolated ray intersections polymorphically. |

Composite contours require `IContourPath` curves because every boundary segment must be finite, directed, and able to participate in enclosure and ray-intersection queries. The ray overload is declared directly by `IContourPath` because composite contours dispatch it across heterogeneous paths. Binary intersections on concrete geometry types, including concrete contour types, are otherwise exposed as extension methods; `IContour` itself does not declare ray intersections.
