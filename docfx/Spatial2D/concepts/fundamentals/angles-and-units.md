# Angles and Units

Akeldov.Math.Spatial2D expresses angles in radians by default. Linear measurements use the
caller-selected world coordinate unit, while raster indices and resolutions are integral counts.
Keeping these unit roles explicit prevents subtle errors at API boundaries.

## Use radians by default

An angle parameter or property without a unit suffix is measured in radians. Numeric properties
that expose degrees use an explicit `Deg` suffix, such as `Arc.StartAngleDeg` and
`Arc.EndAngleDeg`; other degree-oriented members state `Degrees` in their name.

Common angles are:

| Direction or turn | Radians | Degrees |
|---|---:|---:|
| Positive X axis | `0` | `0°` |
| Positive Y axis | `MathF.PI / 2f` | `90°` |
| Negative X axis | `MathF.PI` | `180°` |
| Negative Y axis | `3f * MathF.PI / 2f` | `270°` |
| Full turn | `2f * MathF.PI` | `360°` |

For example, a ray at 45 degrees is constructed with a radian angle:

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var ray = new Ray(
    origin: new PointXY(0f, 0f),
    angle: MathF.PI / 4f);

float angleRad = ray.Angle; // PI / 4, approximately 0.7854 rad
```

The name `Angle` does not mean degrees. Pass `45f` only if 45 radians is actually intended.

## Follow the Cartesian rotation convention

Angle zero points along the positive X axis. Positive angles rotate counterclockwise toward the
positive Y axis, and negative angles rotate clockwise:

```csharp
VectorXY counterclockwise = VectorXY.BasisX.Rotate(MathF.PI / 2f);
// Approximately (0, 1)

VectorXY clockwise = VectorXY.BasisX.Rotate(-MathF.PI / 2f);
// Approximately (0, -1)
```

The same convention applies to rotated points, rays, arcs, and oriented rectangles. For an
oriented rectangle, `Rotation` is the counterclockwise rotation of its local X axis from the
world X axis.

## Work with signed angles

<xref:Akeldov.Math.Spatial2D.VectorXY> calculates the signed shortest angle between two
directions with `VectorXY.Angle(from, to)`. Its result follows the `Atan2` range from `-PI` through
`PI`:

```csharp
float toUp = VectorXY.Angle(VectorXY.BasisX, VectorXY.BasisY);
// PI / 2

float toDown = VectorXY.Angle(VectorXY.BasisX, new VectorXY(0f, -1f));
// -PI / 2
```

A positive result means the `to` direction lies counterclockwise from `from` along the shortest
turn. A negative result means the shortest turn is clockwise.

## Normalize only when the contract requires it

Angles separated by a whole turn describe the same direction. Use `NormalizeAngleRad` when a
canonical representation is needed:

```csharp
using Akeldov.Math.Spatial2D.Curves;

float normalized = (-MathF.PI / 2f).NormalizeAngleRad();
// 3 * PI / 2, approximately 4.7124
```

The method returns an angle in the half-open range `[0, 2 * PI)`. It rejects `NaN` and infinity.

Do not assume every angle property is normalized. For example, <xref:Akeldov.Math.Spatial2D.Curves.Ray>
keeps the finite angle supplied to its constructor, so equivalent directions may have values
that differ by one or more full turns.

Circular arcs are different: <xref:Akeldov.Math.Spatial2D.Curves.Arc> normalizes `StartAngle` and
`EndAngle` into `[0, 2 * PI)`. It separately remembers whether input angles differed by a nonzero
whole number of full turns, allowing these geometrically distinct cases:

```csharp
var center = new PointXY(0f, 0f);

var zeroLength = new Arc(center, 2f, 0f, 0f);
var fullCircle = new Arc(center, 2f, 0f, 2f * MathF.PI);

bool emptySweep = zeroLength.IsFullCircle; // false
bool fullSweep = fullCircle.IsFullCircle;  // true
```

Both arcs expose normalized start and end angles of zero, so `IsFullCircle` carries essential
sweep information.

## Convert degrees at input and output boundaries

When external data uses degrees, convert it before calling an API whose parameter is in radians:

```csharp
float degrees = 30f;
float radians = degrees * MathF.PI / 180f;

VectorXY direction = VectorXY.BasisX.Rotate(radians);
```

Convert a radian result back to degrees for display or interchange when needed:

```csharp
float displayedDegrees = radians * 180f / MathF.PI; // 30
```

Some circular types provide convenience properties such as `StartAngleDeg` and `EndAngleDeg`.
These properties expose degrees; the corresponding unsuffixed properties and constructor
parameters remain in radians.

Convert once at a system boundary and keep internal calculations in radians. Repeated conversion
adds rounding noise and makes unit mistakes harder to spot.

## Pass only finite angles

Public rotation and geometry APIs that accept an angle require a finite value. `NaN` and positive
or negative infinity cause `ArgumentOutOfRangeException`:

```csharp
var point = new PointXY(2f, 0f);
var pivot = new PointXY(1f, 0f);

PointXY rotated = point.Rotate(pivot, MathF.PI / 2f);
// Approximately (1, 1)
```

Angles do not generally need to be pre-normalized. Trigonometric rotation is periodic, and APIs
that require a canonical range normalize according to their own documented contract.

## Choose consistent world units

Spatial2D does not prescribe meters, pixels, or any other physical unit. The following values are
measured in the world coordinate unit chosen by the caller:

- `PointXY` coordinates and `VectorXY` displacements or sizes;
- distances, lengths, radii, widths, and heights;
- raster origins, world-space sizes, and cell sizes;
- geometric tolerances passed to distance-based operations.

These quantities must use the same scale within a calculation. If point coordinates are meters,
then a circle radius, an offset, and a distance tolerance used with those points must also be in
meters.

```csharp
using Akeldov.Math.Spatial2D.Contours;

// Every linear value in this example is interpreted as meters.
var centerMeters = new PointXY(12f, 5f);
float radiusMeters = 2f;
var circle = new Circle(centerMeters, radiusMeters);

float distanceMeters = circle.Distance(new PointXY(15f, 5f)); // 1
```

Angles are independent of the selected world unit. Scaling a model changes its lengths but not
its directions or rotation angles.

## Distinguish counts and normalized values

`VectorXYInt` values used as raster indices or resolutions are counts, not world-space lengths.
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> connects those counts to world units:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(10f, 6f),       // World units
    resolution: new VectorXYInt(5, 3)); // Cell counts

VectorXY cellSize = geometry.CellSize; // (2, 2) world units per cell
```

Values explicitly described as normalized are dimensionless, usually relative to a range such
as `[0, 1]`. Do not mix a normalized coordinate directly with a world-space length without first
applying the scale and origin defined by the relevant API.

For coordinate roles and raster index conventions, see [Coordinate System](coordinate-system/index.md).
