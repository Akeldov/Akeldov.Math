# Vectors

Akeldov.Math.Spatial2D provides floating-point and integer two-dimensional vectors. Both are immutable value types with Cartesian X and Y components, arithmetic operators, length calculations, transformations, and binary serialization helpers.

## Vector Types

| | [`VectorXY`](vectorxy.md) | [`VectorXYInt`](vectorxyint.md) |
|---|---|---|
| Components | `float` | `int` |
| Primary use | Directions, offsets, sizes, and continuous geometry | Grid dimensions, raster resolutions, and discrete offsets |
| Arithmetic | Preserves fractional values | Preserves integral values; division uses integer division |
| Length | `float` | `float` |
| Rotation | Returns `VectorXY` | Returns `VectorXY` |
| Rounding | Can be rounded to `VectorXYInt` | Not required; components are already integral |
| Conversion | Implicit from `VectorXYInt` | Explicit from `VectorXY` |

Use `VectorXY` for geometric calculations and whenever an operation can produce fractional coordinates. Use `VectorXYInt` when integer components are part of the domain contract rather than an incidental representation.

