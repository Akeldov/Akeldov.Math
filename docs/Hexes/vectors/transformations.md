# Transformations

Transformation helpers rotate and transform QRS and related XY vector values.

## Sixfold Rotation

- Rotate QRS vectors by `SixfoldAngle`.
- Rotate integer QRS indexes by sixfold directions.
- Reject invalid sixfold angle values.

## Linear Transformations

- Apply linear transformation helpers to QRS vectors.
- Apply matching helpers to XY and XY integer vectors.
- Keep transformations reusable across topology and geometry APIs.

## Affine Transformations

- Transform XY vectors around pivots.
- Rotate around pivot points.
- Compose common Spatial2D-style vector operations with hex-grid coordinates.
