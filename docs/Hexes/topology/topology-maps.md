# Topology Maps

Topology maps store adjacency values indexed by hex coordinates.

## `IndexSeptupletMap`

- Stores a main index and six adjacent indexes.
- Exposes the common hex map contract.
- Preserves logical neighbor indexes even when some neighbors are outside a bounded field.

## `IndexPartialSeptupletMap`

- Stores adjacency with presence flags.
- Marks which neighboring positions are present.
- Supports bounded fields where edges may be missing.

## Map Behavior

- Uses layout-specific adjacency.
- Exposes width, height, layout, and count metadata.
- Validates map indexes before access.
