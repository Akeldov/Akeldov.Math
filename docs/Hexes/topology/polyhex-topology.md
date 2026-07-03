# Polyhex Topology

Polyhex topology represents connected or bounded sets of hex cells.

## Masks

- Convert integer masks to boolean masks.
- Convert boolean masks into polyhex values.
- Copy input masks so polyhex instances remain independent.

## Builders

- `PolyhexBuilder` creates mutable polyhex masks before freezing them into `Polyhex`.
- Builders support indexed cell access.
- Built polyhex values are independent from the builder.

## Topological Operations

- Extend a polyhex mask.
- Extract contour cells.
- Serialize and deserialize polyhex stamps.
