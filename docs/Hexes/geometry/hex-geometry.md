# Hex Geometry

Hex geometry maps hex indexes to world-space Spatial2D primitives.

## Centers

- Calculate hex centers in world coordinates.
- Apply radius, apothem, origin, and layout.
- Use `HexCenterMap` when centers are needed for every index in a field.

## Vertices

- Calculate hex vertices in world coordinates.
- Get normalized hex vertices for reusable unit geometry.
- Find the closest hex vertex to a point.

## Bounds

- Calculate hex bounding boxes.
- Use `HexOrientedRectangle` for layout-aware rectangular geometry.
- Validate finite geometry parameters before building shapes.
