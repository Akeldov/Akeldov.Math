# `HexMap<TValue>`

`HexMap<TValue>` is the general-purpose hex-indexed value storage type.

## Storage

- Stores values for a rectangular hex index domain.
- Uses topology-backed indexing.
- Exposes layout and dimension metadata.

## Access

- Supports coordinate-based value lookup.
- Validates out-of-bounds indexes.
- Reuses shared flat-index mapping.

## Specialization

- Higher-level APIs can build domain-specific maps on top of the same shape.
- The map type is not tied to topology, geometry, or chromatization values.
