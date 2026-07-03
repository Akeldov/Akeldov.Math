# Shared Map Behavior

Shared map behavior keeps hex-indexed storage predictable across domains.

## Indexing

- Index by hex coordinates.
- Convert coordinate pairs to flat indexes.
- Use the active layout where neighbor logic is required.

## Bounds

- Validate indexes before access.
- Reject indexes outside width and height.
- Keep failures explicit for callers.

## Bounding Boxes

- Common helpers calculate map bounding boxes.
- Geometry-aware maps can use these helpers to align world-space and index-space extents.
