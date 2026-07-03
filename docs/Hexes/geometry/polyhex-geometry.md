# Polyhex Geometry

Polyhex geometry converts hex masks into Spatial2D contours and regions.

## `PolyhexGeometry`

- Combines polyhex topology with layout and size parameters.
- Provides geometry-aware access to a polyhex field.
- Builds on `Polyhex` topology data.

## Regions

- Convert polyhex masks to Spatial2D regions.
- Preserve holes when contour topology contains them.
- Generate closed contour geometry.

## Offsets

- Generate apothem-offset contours.
- Use apothem-radius joins for convex corners.
- Preserve self-intersection-free contour output for supported shapes.
