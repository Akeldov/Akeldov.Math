# Layouts

Layouts define how hex indexes map to rows, columns, world-space centers, and neighboring cells.

## Layout Values

- `OddR`.
    - Odd row offset layout.
    - Row-oriented layout.
- `EvenR`.
    - Even row offset layout.
    - Row-oriented layout.
- `OddQ`.
    - Odd column offset layout.
    - Column-oriented layout.
- `EvenQ`.
    - Even column offset layout.
    - Column-oriented layout.

## Orientation

- `HexOrientation`.
    - Identifies pointy-top and flat-top hex layouts.
    - Separates row-oriented and column-oriented helpers.
- Layout extension helpers.
    - Detect row-based layouts.
    - Detect column-based layouts.
    - Reject unsupported layout values.

## Coordinate Rules

- Layout-specific offset conversion.
- QRS to XY index mapping.
- XY index to QRS mapping.
- Neighbor offset rules for odd and even rows or columns.

## Geometry Rules

- Hex center calculation.
- Hex vertex calculation.
- Normalized hex vertices.
- Layout-aware bounding boxes.
- Radius and apothem conversion helpers.
