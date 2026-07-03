# Vector Types

QRS vector types are the primary coordinate values used by Hexes APIs.

## `VectorQRS`

- Stores fractional QRS coordinates.
- Keeps `S` derived from `Q` and `R`.
- Supports vector arithmetic and equality.
- Can be converted to integer QRS indexes when discretized.

## `VectorQRSInt`

- Stores integer QRS indexes.
- Validates that the QRS components keep the zero-sum cube-coordinate invariant.
- Supports checked integer arithmetic.
- Converts to and from the fractional QRS representation.
