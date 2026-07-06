# Layouts

Layouts define how hex indexes map to rows, columns, world-space centers, and neighboring cells.

## Layout Values

`Layout` identifies the offset-coordinate convention used by a hex field.

### Row Layouts

- `OddR`.
    - Odd row offset layout.
    - Row-oriented layout.
    - `HexOrientation.PointyTop`.
- `EvenR`.
    - Even row offset layout.
    - Row-oriented layout.
    - `HexOrientation.PointyTop`.

### Column Layouts

- `OddQ`.
    - Odd column offset layout.
    - Column-oriented layout.
    - `HexOrientation.FlatTop`.
- `EvenQ`.
    - Even column offset layout.
    - Column-oriented layout.
    - `HexOrientation.FlatTop`.
