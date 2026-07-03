# Coordinate Rules

Coordinate rules define how layout offsets map between QRS and XY indexes.

## Offset Conversion

- Apply odd row offsets for `OddR`.
- Apply even row offsets for `EvenR`.
- Apply odd column offsets for `OddQ`.
- Apply even column offsets for `EvenQ`.

## QRS and XY Mapping

- Convert QRS indexes to XY indexes.
- Convert XY indexes to QRS indexes.
- Keep QRS coordinates layout-neutral when possible.

## Neighbor Rules

- Neighbor offsets depend on row or column parity.
- Edge adjacency uses the active layout.
- Invalid layouts are rejected before adjacency is calculated.
