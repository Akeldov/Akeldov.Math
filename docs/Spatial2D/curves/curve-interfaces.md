# Curve Interfaces

Curve interfaces describe which operations and invariants a type exposes.

| Interface | Meaning |
|---|---|
| `ICurve` | Can measure distance, project a point, and intersect a ray. |
| `IFiniteCurve` | Has finite length. |
| `IOneEndpointCurve` | Has one endpoint, such as a ray origin. |
| `ITwoEndpointCurve` | Has two unordered endpoints. |
| `IParameterizedCurve` | Supports `GetPoint` and `ProjectWithParameter`. |
| `IPath` | A parameterized curve with two endpoints. |
| `IFinitePath` | A finite path suitable for contours. |
| `IRayPath` | A half-infinite parameterized path. |

Contours require `IFinitePath` curves because each boundary segment must be finite and have ordered start and end points.
