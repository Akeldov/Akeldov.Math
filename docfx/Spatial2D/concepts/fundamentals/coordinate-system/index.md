# Coordinate System

Akeldov.Math.Spatial2D uses a two-dimensional Cartesian coordinate system. Positions,
displacements, directions, sizes, and discrete grid coordinates all have `X` and `Y` components,
but their types preserve the different meanings of those values.

## Cartesian axes

The positive X axis points to the right and the positive Y axis points upward. The standard
floating-point basis vectors are:

```csharp
using Akeldov.Math.Spatial2D;

VectorXY right = VectorXY.BasisX; // (1, 0)
VectorXY up = VectorXY.BasisY;    // (0, 1)
```

Positive signed angles rotate counterclockwise from one direction to another. Spatial2D angles
are expressed in radians unless a member has an explicit unit suffix. See
[Angles and Units](../angles-and-units.md) for the complete convention.

The coordinate components themselves have no built-in physical unit. A model may interpret them
as meters, pixels, tiles, or another unit, but all values participating in one geometric
calculation must use a consistent scale.

## Choose the value by meaning

Use the type that describes the role of a value, not merely the numeric form of its components:

| Type | Meaning | Typical uses |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.PointXY> | A position in continuous space | Curve endpoints, region centers, samples, intersections |
| <xref:Akeldov.Math.Spatial2D.VectorXY> | A continuous direction or displacement | Offsets, directions, world-space sizes, transformations |
| <xref:Akeldov.Math.Spatial2D.VectorXYInt> | An integral pair interpreted by the receiving API | Raster indices, resolutions, dimensions, discrete offsets |

A point and a vector can contain the same numeric components while expressing different
geometric ideas. A discrete index can likewise have components equal to a world-space point
without referring to the same location.

## Compose positions and displacements

The type system reflects the standard affine relationships between points and vectors:

```csharp
var start = new PointXY(2f, 3f);
var offset = new VectorXY(4f, -1f);

PointXY end = start + offset;       // (6, 2)
VectorXY displacement = end - start; // (4, -1)
PointXY restored = end - offset;    // (2, 3)
```

The useful relationships are:

```text
point + vector = point
point - vector = point
point - point  = vector
vector + vector = vector
```

Adding two points is intentionally unsupported because the result has no unambiguous geometric
meaning. Conversion between `PointXY` and `VectorXY` is explicit for the same reason: the numeric
components are preserved, but the role changes visibly at the call site.

## Separate continuous and discrete space

`PointXY` and `VectorXY` use `float` components for continuous geometric calculations.
`VectorXYInt` uses `int` components when integral values are part of the contract.

Keep a calculation continuous until an API actually requires a discrete value. Conversion from
`VectorXY` to `VectorXYInt` can truncate or round components and therefore requires an explicit
choice. The [Vectors](vectors.md) page explains those conversion rules.

Raster grids demonstrate the boundary between the two spaces. A raster has an integral
resolution and zero-based cell indices, while its bounds and sample positions may exist in
continuous world space:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(-2f, -1f),
    size: new VectorXY(8f, 4f),
    resolution: new VectorXYInt(4, 2));

PointXY firstCellCenter = geometry.GetCellCenter(0, 0); // (-1, 0)
```

<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> places the discrete grid in world
space. Its origin is the lower-left boundary, and each cell is sampled at its center. Increasing
the cell's `X` or `Y` index follows the corresponding positive world axis. See
[Discrete Indices](discrete-indices.md) for bounds, iteration, and mapping conventions.

## Preserve semantic distinctions

Prefer API signatures and local names that expose the intended role:

```csharp
PointXY position = new PointXY(3f, 2f);
VectorXY translation = new VectorXY(3f, 2f);
VectorXYInt cellIndex = new VectorXYInt(3, 2);
```

Avoid treating these values as interchangeable coordinate pairs. Preserving their roles makes
invalid operations harder to express and makes unit or discretization boundaries easier to
review.

## Topics

- [Points](points.md) — positions, distance, interpolation, and point transformations.
- [Vectors](vectors.md) — continuous and integer vectors, direction operations, and conversion.
- [Discrete Indices](discrete-indices.md) — raster addressing, resolutions, offsets, and
  world-space mapping.
