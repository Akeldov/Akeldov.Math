# Linear Curves

Linear curves describe straight geometry in 2D space: infinite lines, half-lines, and bounded segments.
They live in the `Akeldov.Math.Spatial2D.Curves` namespace and share the same projection, distance, and ray-intersection conventions as the [curves overview](../curves.md).

Curve coordinates and distances are measured in world coordinate units.
`Ray` angle parameters and properties are expressed in radians.

<p>
  <img alt="Line distance raster" src="../../../../assets/spatial2d/curves/line-distance.png" width="160">
  <img alt="Parameterized line distance raster" src="../../../../assets/spatial2d/curves/parameterized-line-distance.png" width="160">
  <img alt="Ray distance raster" src="../../../../assets/spatial2d/curves/ray-distance.png" width="160">
  <img alt="Segment distance raster" src="../../../../assets/spatial2d/curves/segment-distance.png" width="160">
  <img alt="Parameterized segment distance raster" src="../../../../assets/spatial2d/curves/parameterized-segment-distance.png" width="160">
</p>

| Type | Geometry | Coordinate Domain | Use When |
|---|---|---|---|
| `Line` | Infinite line | - | You only need geometric distance, projection, or side tests. |
| `ParameterizedLine` | Infinite directed line | `(-inf, +inf)` | You need a signed coordinate along an infinite line. |
| `Ray` | Half-line from an origin | `[0, +inf)` | You need a directed path with a fixed start point. |
| `Segment` | Bounded line segment | - | Endpoint order should not matter. |
| `ParameterizedSegment` | Bounded directed segment | `[0, Length]` | You need traversal direction or distance along the segment. |

## Choosing a Type

Use `Line` and `Segment` for pure geometry.
They answer distance, projection, and intersection questions without assigning an orientation to the curve.

Use `ParameterizedLine`, `Ray`, and `ParameterizedSegment` when you need a curve coordinate.
`ProjectWithParameter` returns the closest point, the distance to the curve, and the coordinate of that point along the curve.
`GetPoint` does the inverse: it returns a point at a specific curve coordinate.

`Segment` compares equal even when its endpoints are supplied in the opposite order.
`ParameterizedSegment` keeps `StartPoint` and `EndPoint` distinct, so reversing the endpoints reverses the coordinate domain.

## Lines

`Line` represents an infinite geometric line.
It can be created from two distinct points or from an implicit equation `ax + by + c = 0`.
The equation coefficients are normalized, so equivalent equations compare as the same line.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var line = new Line(
    new PointXY(0f, 2f),
    new PointXY(6f, 2f));

var sameLine = new Line(a: 0f, b: 1f, c: -2f);

CurveProjection projection = line.Project(new PointXY(4f, 5f));

PointXY closestPoint = projection.ProjectedPoint; // (4, 2)
float distance = projection.Distance;             // 3
```

`ParameterizedLine` adds an `Origin` and `Direction` to the same infinite geometry.
Coordinates are signed and are measured from `Origin` along `Direction`.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var geometricLine = new Line(
    new PointXY(0f, 2f),
    new PointXY(6f, 2f));

var path = new ParameterizedLine(
    geometricLine,
    referencePoint: new PointXY(3f, 10f));

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(5f, 5f));

PointXY projectedPoint = projection.ProjectedPoint;  // (5, 2)
float curveCoordinate = projection.CurveCoordinate;  // 2
PointXY beforeOrigin = path.GetPoint(-1f);           // (2, 2)
```

You can choose the coordinate origin with an explicit reference point or with `LineReferencePointMode`.
The reference point is projected onto the line, and that projected point becomes `Origin`.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var centered = new ParameterizedLine(
    new PointXY(0f, 2f),
    new PointXY(6f, 2f),
    LineReferencePointMode.Midpoint);
```

## Rays

`Ray` starts at `Origin` and extends forever in `Direction`.
Its coordinate domain starts at `0`; points that would project behind the origin clamp to the origin.

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var ray = new Ray(
    origin: new PointXY(0f, 0f),
    angle: MathF.PI / 4f);

ParameterizedCurveProjection projection =
    ray.ProjectWithParameter(new PointXY(-1f, 2f));

PointXY start = ray.GetPoint(0f);
PointXY fiveUnitsAlongRay = ray.GetPoint(5f);
```

The `angle` argument and `Angle` property are in radians.
Use `Direction` when you want the normalized vector form.

## Segments

`Segment` is a finite curve between two endpoints.
It is endpoint-order agnostic, but endpoint inclusion is still preserved.
Endpoint inclusion matters for ray intersections at exact endpoints.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var closed = new Segment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

var openAtStart = new Segment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f),
    includesEndpointA: false,
    includesEndpointB: true);

CurveProjection projection = closed.Project(new PointXY(4f, 3f));
Segment shorter = closed.Shorten(1f);
Segment longer = closed.Extend(2f);
```

`ParameterizedSegment` is the directed version.
Its coordinate starts at `0` at `StartPoint` and ends at `Length` at `EndPoint`.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

ParameterizedCurveProjection projection =
    path.ProjectWithParameter(new PointXY(4f, 3f));

float curveCoordinate = projection.CurveCoordinate; // 4
PointXY halfway = path.GetPoint(path.Length * 0.5f);
Segment geometricSegment = (Segment)path;
```

Degenerate segments are allowed.
For a zero-length `Segment`, projection returns the endpoint.
For a zero-length `ParameterizedSegment`, coordinate `0` returns `StartPoint`.

## Ray Intersections

`GetRayIntersections` returns a new mutable list of intersection points in the forward direction of the ray.
For collinear overlaps, linear curves return the first point encountered by the ray rather than a segment of overlap.

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var wall = new Segment(
    new PointXY(2f, -1f),
    new PointXY(2f, 1f));

var ray = new Ray(new PointXY(0f, 0f));

List<PointXY> hits = wall.GetRayIntersections(ray);
```

The `geometryEpsilon` argument is measured in world coordinate units and controls comparisons near collinear overlaps, endpoints, and nearly parallel lines.
