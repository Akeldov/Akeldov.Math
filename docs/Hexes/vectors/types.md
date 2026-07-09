# Vector Types

QRS vector types are the primary coordinate values used by Hexes APIs.

## `VectorQRS`

- Stores fractional QRS coordinates.
- Corresponds to cube or axial coordinates of a point in the hex coordinate plane.
- Keeps `S` derived from `Q` and `R`.
- Supports vector arithmetic and equality.
- Can be converted to integer QRS indexes when discretized.

## `VectorQRSInt`

- Stores integer QRS coordinates.
- Represents either rounded integer coordinates or QRS-form hex indexes.
- Validates that the QRS components keep the zero-sum cube-coordinate invariant.
- Supports checked integer arithmetic.
- Converts to and from the fractional QRS representation.
- When used as a hex index, converts to and from row or column storage indexes
  according to the selected `Layout`.
