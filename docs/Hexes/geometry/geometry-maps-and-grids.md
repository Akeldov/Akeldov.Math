# Geometry Maps and Grids

Geometry maps and grids sample Spatial2D values from a hex field.

## `HexCenterMap`

- Maps each hex index to its world-space center.
- Implements the common hex map contract.
- Preserves layout and geometric parameters.

## `BarycentricTripletGrid`

- Samples barycentric weights for vertex triplets.
- Supports geometry rasterization workflows.
- Exposes sampled grid metadata.

## `BarycentricPartialTripletGrid`

- Samples barycentric weights with presence flags.
- Handles missing neighboring cells at field boundaries.
- Supports rasterization of partial vertex neighborhoods.
