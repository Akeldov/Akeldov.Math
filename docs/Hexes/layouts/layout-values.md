# Layout Values

`Layout` identifies the offset-coordinate convention used by a hex field.

## Row Layouts

- `OddR`.
    - Odd row offset layout.
    - Row-oriented layout.
- `EvenR`.
    - Even row offset layout.
    - Row-oriented layout.

## Column Layouts

- `OddQ`.
    - Odd column offset layout.
    - Column-oriented layout.
- `EvenQ`.
    - Even column offset layout.
    - Column-oriented layout.

## Usage

- Choose a layout before converting between QRS and XY coordinates.
- Use the same layout for topology, geometry, maps, and grids that describe the same field.
