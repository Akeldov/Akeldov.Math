# Adjacency

Adjacency helpers navigate from a hex index to neighboring hex indexes.

## Edge Adjacency

- Find the neighboring hex index for a selected edge.
- Use layout-aware row or column offsets.
- Reject unsupported layout values.

## Vertex Adjacency

- Find the pair of hexes adjacent to a vertex.
- Find the triplet of hexes that meet at a vertex.
- Preserve left/right or adjacent ordering for downstream grids.

## Rings

- Calculate ring offsets around a hex.
- Use relative offsets for caller-owned traversal.
