# Coordinate Conversions

Coordinate conversion helpers connect QRS, XY indexes, and Spatial2D coordinates.

## QRS and XY Indexes

- Convert QRS indexes to XY indexes.
- Convert XY indexes back to QRS indexes.
- Keep odd/even row and column offset rules layout-specific.

## Spatial2D Coordinates

- Convert Spatial2D `PointXY` values to QRS coordinates.
- Convert QRS values to Spatial2D vectors.
- Use layout orientation to choose row-oriented or column-oriented formulas.

## Layout Awareness

- Row layouts use row offset rules.
- Column layouts use column offset rules.
- Unsupported layout values are rejected.
