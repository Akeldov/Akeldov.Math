# Vectors

Akeldov.Math.Hexes provides fractional and integer QRS vectors for continuous hex-grid calculations and discrete hex indexes. Both are immutable value types with `Q`, `R`, and `S` components that preserve the cube-coordinate invariant `Q + R + S = 0`.

## Vector Types

| | [`VectorQRS`](vectors/types.md#vectorqrs) | [`VectorQRSInt`](vectors/types.md#vectorqrsint) |
|---|---|---|
| Components | `float` | `int` |
| Primary use | Fractional positions, interpolation, and continuous transformations | Hex indexes, neighbor offsets, and discrete transformations |
| Stored state | `Q` and `R`; `S` is derived | `Q` and `R`; `S` is derived or validated |
| Arithmetic | Preserves fractional values | Preserves integral values and checks overflow |
| Division | Uses floating-point division | Uses integer division |
| Rotation | Arbitrary radian angles or `SixfoldAngle` | `SixfoldAngle`, or arbitrary angles returning `VectorQRS` |
| Discretization | Rounds to the nearest `VectorQRSInt` hex index | Components are already integral |
| Conversion | Implicit from `VectorQRSInt` | Explicit from `VectorQRS`; layout-aware conversion to XY indexes |

Use `VectorQRS` while coordinates may be fractional or an operation can produce a non-integral result. Use `VectorQRSInt` when the value identifies a discrete hex or represents an exact integer offset in QRS space.

## Coordinate Model

QRS coordinates are the cube-coordinate form of axial hex coordinates. Only two components are independent; constructors derive or validate the third component so every value remains on the `Q + R + S = 0` plane.

The selected [`Layout`](layouts.md) affects conversion to offset-storage indexes and the orientation of QRS axes in Spatial2D coordinates. Odd and even variants share the same continuous axes; they differ only in how alternating rows or columns are offset.

See [Coordinate Conversions](vectors/coordinate-conversions.md) for layout-aware index conversion and diagrams of the QRS basis.

## Topics

- [Vector Types](vectors/types.md)
- [Coordinate Conversions](vectors/coordinate-conversions.md)
- [Discretization](vectors/discretization.md)
- [Transformations](vectors/transformations.md)
- [Serialization](vectors/serialization.md)
