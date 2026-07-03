# Vectors

Hexes uses QRS axial coordinates as the main coordinate model for hex-grid math.

## Types

- `VectorQRS`.
    - Stores fractional QRS coordinates.
    - Supports vector arithmetic and equality.
    - Can be converted to integer QRS indexes.
- `VectorQRSInt`.
    - Stores integer QRS indexes.
    - Keeps the QRS sum invariant.
    - Supports checked integer arithmetic.

## Coordinate Conversions

- QRS to XY index conversion.
- XY index to QRS conversion.
- Spatial2D `PointXY` to QRS conversion.
- QRS to Spatial2D vector conversion.

## Discretization

- Fractional QRS coordinates can be rounded to the nearest hex index.
- Layout-aware discretization keeps row and column offset rules outside the QRS value itself.
- Large and non-finite coordinate cases are validated before integer conversion.

## Transformations

- QRS vectors can be rotated by `SixfoldAngle`.
- QRS vectors can participate in linear and affine transformations.
- Related XY and XY integer vector helpers expose matching transformation operations.

## Serialization

- Binary reader helpers read QRS values.
- Binary writer helpers write QRS values.
- Invalid serialized sixfold angle values are rejected during reading.
