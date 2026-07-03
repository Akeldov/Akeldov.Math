# Maps

Maps are general hex-indexed value containers. Domain-specific maps are documented with their domain pages.

## `IHexMap<TValue>`

- Common contract for hex-indexed value maps.
- Exposes width, height, layout, and count metadata.
- Supports coordinate-based value access.
- Keeps the public API independent from a specific value domain.

## `HexMap<TValue>`

- General-purpose hex-indexed value storage.
- Uses topology-backed indexing.
- Stores values for a rectangular hex index domain.
- Can be specialized by higher-level topology, geometry, chromatization, or partitioning APIs.

## Shared Map Behavior

- Indexing by hex coordinates.
- Flat index mapping.
- Bounds validation.
- Common map bounding-box operations.
