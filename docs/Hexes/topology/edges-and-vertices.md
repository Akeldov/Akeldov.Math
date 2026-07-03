# Edges and Vertices

Edges and vertices identify local positions around a single hex.

## `HexEdge`

- Represents the six edges of a hex.
- Selects one adjacent hex index.
- Drives edge-neighbor lookup.

## `HexVertex`

- Represents the six vertices of a hex.
- Selects adjacent edge pairs.
- Drives vertex pair and vertex triplet lookup.

## Local Order

- Edge and vertex values follow the library's sixfold order.
- The active layout controls how local positions map to neighboring indexes.
