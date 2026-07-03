# Geometry

Geometry APIs map topology and indexes into Spatial2D coordinates, contours, regions, rasters, and partitions.

## Hex Geometry

- Hex centers in world coordinates.
- Hex vertices in world coordinates.
- Normalized hex vertices.
- Closest hex vertex lookup.
- Hex bounding boxes.
- `HexOrientedRectangle`.

## Geometry Maps and Grids

- `HexCenterMap`.
    - Maps each hex index to its world-space center.
- `BarycentricTripletGrid`.
    - Samples barycentric weights for vertex triplets.
- `BarycentricPartialTripletGrid`.
    - Samples barycentric weights with presence flags.

## Polyhex Geometry

- `PolyhexGeometry`.
    - Combines polyhex topology with geometric parameters.
- Polyhex to Spatial2D region conversion.
- Contour generation from masks.
- Apothem-offset contours and regions.

## Field Parameters

- Parameter reconstruction for hex fields.
- Apothem, radius, origin, and dimension helpers.
- Validation for finite geometry parameters.

## Partitioning

- Weighted Voronoi partitioning over hex centers.
- `VoronoiCell`.
- `VoronoiHexPartitionMap`.
- Empty-cell and weighted-site handling.
