# PointXY

`PointXY` represents a two-dimensional position with single-precision floating-point coordinates. Use it for world-space positions, curve endpoints, region centers, sample locations, and other values that identify a point rather than an offset or direction.

Spatial2D uses conventional Cartesian coordinates. Geometric distances and curve coordinates are measured in the same world units as the source points. Raster grids use `PointXY` values for world-space positions and bounds.

## Construction and coordinates

```csharp
using Akeldov.Math.Spatial2D;

var point = new PointXY(3.5f, 8f);

float x = point.X; // 3.5
float y = point.Y; // 8
```

`PointXY` is an immutable value type. The constructor rejects `NaN` coordinates with `ArgumentOutOfRangeException`. It does not reject infinity, although geometric APIs that require finite positions validate it at their boundaries.

Coordinates can be obtained through deconstruction:

```csharp
var (x, y) = new PointXY(3.5f, 8f);
```

## Position interface

`PointXY` implements `IHasPosition2D`. Its `Position` property returns the point itself, so a point can be passed directly to APIs that operate on positioned objects.

```csharp
IHasPosition2D positioned = new PointXY(2f, 4f);
PointXY position = positioned.Position; // (2, 4)
```

## Point and vector arithmetic

A vector translates a point. Subtracting two points produces the displacement vector from the right-hand point to the left-hand point.

```csharp
var point = new PointXY(3f, 4f);
var offset = new VectorXY(1f, 2f);

PointXY movedA = point + offset; // (4, 6)
PointXY movedB = offset + point; // (4, 6)
PointXY movedBack = point - offset; // (2, 2)

VectorXY displacement = movedA - point; // (1, 2)
```

Adding two points is intentionally not supported: a position can be translated by a vector, while the difference between two positions is a vector.

## Equality and tolerance

`PointXY` implements `IEquatable<PointXY>`. `Equals`, `==`, and `!=` compare coordinates exactly:

```csharp
var a = new PointXY(1f, 2f);
var b = new PointXY(1f, 2f);

bool equal = a == b;       // true
bool different = a != b;  // false
```

Use `AlmostEquals` when points produced by floating-point calculations should be compared by Euclidean distance. The tolerance is inclusive and defaults to `GeometryConstants.GeometryEpsilon`.

```csharp
bool almostEqual = new PointXY(1f, 2f).AlmostEquals(
    new PointXY(1.000001f, 2f));

bool customTolerance = new PointXY(1f, 2f).AlmostEquals(
    new PointXY(1.01f, 2f),
    epsilon: 0.02f); // true
```

Exact equality and the matching hash code make `PointXY` suitable for use as a dictionary key. Use tolerance-based comparison explicitly when geometric proximity is intended.

## Distance

`Distance` returns the Euclidean distance between two points. `SquaredDistanceTo` avoids the square root and is useful when distances only need to be compared.

```csharp
var a = new PointXY(0f, 0f);
var b = new PointXY(3f, 4f);

float distance = a.Distance(b);                 // 5
float squaredDistance = a.SquaredDistanceTo(b); // 25
```

`PointXY` also implements `IPointDistanceProvider`, allowing a point to act as a distance source in APIs that accept that interface.

## Linear interpolation and extrapolation

`LerpTo` moves from the source point toward a target point using parameter `t`.

```csharp
var source = new PointXY(0f, 0f);
var target = new PointXY(10f, 4f);

PointXY start = source.LerpTo(target, 0f);   // (0, 0)
PointXY middle = source.LerpTo(target, 0.5f); // (5, 2)
PointXY end = source.LerpTo(target, 1f);     // (10, 4)
PointXY beyond = source.LerpTo(target, 1.5f); // (15, 6)
```

Values between `0` and `1` interpolate between the points. Values outside that range extrapolate along the same line. A `NaN` or infinite `t` throws `ArgumentOutOfRangeException`.

## Conversion to and from VectorXY

Conversion between a point and its coordinate vector is explicit because points and vectors have different geometric meanings.

```csharp
var point = new PointXY(3f, 2f);
VectorXY coordinates = (VectorXY)point; // (3, 2)

var vector = new VectorXY(5f, 4f);
PointXY position = (PointXY)vector; // (5, 4)
```

## Rotation around a pivot

`Rotate` rotates a point around a specified pivot. Angles are expressed in radians.

```csharp
var point = new PointXY(2f, 0f);
var pivot = new PointXY(1f, 0f);

PointXY rotated = point.Rotate(pivot, MathF.PI / 2f);
// approximately (1, 1)
```

The pivot can be a `PointXY` or `VectorXYInt`. A `NaN` or infinite angle throws `ArgumentOutOfRangeException`.

## Affine transformations

`Transform` rotates a point around the origin and then applies an offset:

```csharp
var point = new PointXY(1f, 0f);

PointXY transformed = point.Transform(
    angle: MathF.PI / 2f,
    offset: new VectorXY(3f, 4f));
// approximately (3, 5)
```

An overload with a scale factor applies the operations in this order:

1. Uniformly scale relative to the origin.
2. Rotate around the origin using an angle in radians.
3. Apply the offset.

```csharp
PointXY transformed = new PointXY(1f, 0f).Transform(
    scaleFactor: 2f,
    angle: MathF.PI / 2f,
    offset: new VectorXY(3f, 4f));
// approximately (3, 6)
```

Both transform forms accept either `VectorXY` or `VectorXYInt` as the offset. A `NaN` or infinite angle throws `ArgumentOutOfRangeException`.

## String representation

`ToString` formats the coordinates using invariant culture:

```csharp
string text = new PointXY(3.5f, 8f).ToString(); // "(3.5, 8)"
```
