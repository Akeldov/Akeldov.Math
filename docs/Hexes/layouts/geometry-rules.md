# Geometry Rules

Geometry rules map layout indexes to Spatial2D coordinates.

## Centers

- Calculate hex centers with radius and apothem.
- Apply a hex field origin.
- Preserve the selected layout's row or column spacing.

## Vertices

- Calculate world-space hex vertices.
- Use normalized hex vertices for reusable unit geometry.
- Preserve vertex order for downstream topology and rasterization.

## Sizes

- Convert radius to apothem.
- Convert apothem to radius.
- Calculate layout-aware bounding boxes.
