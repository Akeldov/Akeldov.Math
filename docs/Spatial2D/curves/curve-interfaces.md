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
| `IRayIntersectionProvider` | Can report isolated intersections with a ray polymorphically. |
| `IContourPath` | A finite path with both spatial-query capabilities required by composite contours. |

Composite contours require `IContourPath` curves because every boundary segment must be finite, directed, and able to participate in enclosure and ray-intersection queries. Binary intersections on concrete geometry types are exposed as extension methods.
