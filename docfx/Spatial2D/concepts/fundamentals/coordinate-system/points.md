# Points

A <xref:Akeldov.Math.Spatial2D.PointXY> represents a position in two-dimensional
Cartesian space. Its `X` and `Y` coordinates are single-precision floating-point values measured
in the same world units as the geometry that uses the point.

Use points for locations such as curve endpoints, region centers, sample positions, and
intersection results. Use [vectors](vectors.md) for directions, offsets, and displacements.

## Create and inspect a point

Create a point by passing its coordinates to the constructor:

```csharp
using Akeldov.Math.Spatial2D;

var point = new PointXY(3.5f, 8f);

float x = point.X; // 3.5
float y = point.Y; // 8
```

`PointXY` is a `readonly struct` with structural value semantics. Assigning it to a variable or
passing it to a method copies its coordinates, and equality is determined by coordinate values
rather than object identity. Once created, its coordinates cannot change; an operation that
moves or transforms a point returns a new value.

Coordinates can also be obtained through deconstruction:

```csharp
var (x, y) = new PointXY(3.5f, 8f);
```

The constructor rejects `NaN` coordinates. It permits infinity, but geometry APIs that require a
finite position validate that requirement at their public boundary.

## Points and vectors have different roles

A point identifies a location. A vector describes the displacement between locations or a
translation to apply to a location. Spatial2D reflects this distinction in its operators:

```csharp
var start = new PointXY(3f, 4f);
var offset = new VectorXY(1f, 2f);

PointXY end = start + offset;       // (4, 6)
PointXY movedBack = end - offset;   // (3, 4)
VectorXY displacement = end - start; // (1, 2)
```

Adding two points is intentionally not supported. There is no unambiguous geometric meaning for
the sum of two positions, while translating a point by a vector and subtracting two points have
well-defined meanings.

Conversion between a point and a coordinate vector is explicit for the same reason:

```csharp
var point = new PointXY(3f, 2f);
VectorXY coordinates = (VectorXY)point;

var vector = new VectorXY(5f, 4f);
PointXY position = (PointXY)vector;
```

The numeric components are preserved, but the explicit cast makes the change in geometric
meaning visible at the call site.

## Measure the distance between points

<xref:Akeldov.Math.Spatial2D.PointXY.Distance(Akeldov.Math.Spatial2D.PointXY)> returns the
Euclidean distance between two points:

```csharp
var a = new PointXY(0f, 0f);
var b = new PointXY(3f, 4f);

float distance = a.Distance(b); // 5
```

Use
<xref:Akeldov.Math.Spatial2D.PointXYExtensions.SquaredDistanceTo(Akeldov.Math.Spatial2D.PointXY,Akeldov.Math.Spatial2D.PointXY)>
when you only need to compare distances. It avoids the square-root operation:

```csharp
float squaredDistance = a.SquaredDistanceTo(b); // 25
```

`PointXY` also implements <xref:Akeldov.Math.Spatial2D.IPointDistanceProvider>, so a point can
act as a distance source in APIs that accept that abstraction.

## Choose exact or tolerant equality

`PointXY` implements `IEquatable<PointXY>`. `Equals`, `==`, and `!=` compare both coordinates
exactly:

```csharp
var a = new PointXY(1f, 2f);
var b = new PointXY(1f, 2f);

bool equal = a == b;      // true
bool different = a != b; // false
```

Exact equality and the matching hash code make points suitable as dictionary keys. Calculations
with floating-point values can introduce small rounding differences, so geometric proximity
should be tested explicitly with
<xref:Akeldov.Math.Spatial2D.PointXYExtensions.AlmostEquals(Akeldov.Math.Spatial2D.PointXY,Akeldov.Math.Spatial2D.PointXY,System.Single)>:

```csharp
bool almostEqual = new PointXY(1f, 2f).AlmostEquals(
    new PointXY(1.000001f, 2f));

bool withinCustomTolerance = new PointXY(1f, 2f).AlmostEquals(
    new PointXY(1.01f, 2f),
    epsilon: 0.02f); // true
```

The tolerance is an inclusive Euclidean distance and defaults to
`GeometryConstants.GeometryEpsilon`.

## Interpolate and extrapolate

<xref:Akeldov.Math.Spatial2D.PointXYExtensions.LerpTo(Akeldov.Math.Spatial2D.PointXY,Akeldov.Math.Spatial2D.PointXY,System.Single)>
moves from a source point toward a target using parameter `t`:

```csharp
var source = new PointXY(0f, 0f);
var target = new PointXY(10f, 4f);

PointXY start = source.LerpTo(target, 0f);      // (0, 0)
PointXY middle = source.LerpTo(target, 0.5f);  // (5, 2)
PointXY end = source.LerpTo(target, 1f);        // (10, 4)
PointXY beyond = source.LerpTo(target, 1.5f);   // (15, 6)
```

Values from `0` through `1` interpolate along the segment. Values outside that range extrapolate
along the same line. `t` must be finite.

## Rotate and transform points

Use `Rotate` to rotate a point around a pivot. Angles in Spatial2D are expressed in radians by
default:

```csharp
var point = new PointXY(2f, 0f);
var pivot = new PointXY(1f, 0f);

PointXY rotated = point.Rotate(pivot, MathF.PI / 2f);
// Approximately (1, 1)
```

Use `Transform` when the transformation is relative to the origin and includes a translation:

```csharp
var point = new PointXY(1f, 0f);

PointXY transformed = point.Transform(
    angle: MathF.PI / 2f,
    offset: new VectorXY(3f, 4f));
// Approximately (3, 5)
```

The overload with a scale factor applies operations in this order:

1. Uniformly scale relative to the origin.
2. Rotate around the origin by an angle in radians.
3. Apply the translation offset.

```csharp
PointXY transformed = new PointXY(1f, 0f).Transform(
    scaleFactor: 2f,
    angle: MathF.PI / 2f,
    offset: new VectorXY(3f, 4f));
// Approximately (3, 6)
```

Offsets and rotation pivots also have overloads that accept `VectorXYInt`.

## Position-bearing objects

`PointXY` implements <xref:Akeldov.Math.Spatial2D.IHasPosition2D>. Its `Position` property
returns the point itself:

```csharp
IHasPosition2D positioned = new PointXY(2f, 4f);
PointXY position = positioned.Position; // (2, 4)
```

This lets a point participate directly in algorithms that operate on positioned objects, such
as spatial partitioning and influence-source culling.

For the complete member list, see the
<xref:Akeldov.Math.Spatial2D.PointXY> and
<xref:Akeldov.Math.Spatial2D.PointXYExtensions> API references.
