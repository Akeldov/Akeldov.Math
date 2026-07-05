# Akeldov.Math.Spatial2D

Akeldov.Math.Spatial2D is a .NET library for practical two-dimensional geometry, vector math, contours, regions, rasterization, spatial sampling, partitioning, and influence field utilities.

## Features

The library is organized around practical 2D geometry workflows.

### Core Geometry

- Point and vector types: `PointXY`, `VectorXY`, and `VectorXYInt`.
- Vector math helpers for distance, transformations, rounding, scaling, and serialization.
- Curve primitives: `Line`, `Ray`, `Segment`, and `Arc`.
- Projection, distance, intersection, angle, contour, and centroid helpers.

### Boundaries and Areas

- Closed contours made from bounded parameterized curves, including circular and rectangular contours.
- Filled regions with holes and nested contours.
- Axis-aligned, oriented, and normalized rectangle primitives.
- Contour smoothing and corner filleting helpers.
- Signed point-distance contracts for contours and regions.

### Rasterization and Imaging

- Axis-aligned raster grids for sampling geometry into cells.
- Signed-distance rasterizers for contours and regions.
- Mutable spatial and non-spatial grayscale and RGBA rasters.
- Color helpers for normalized values, blending, and temperature heat maps.
- 8-bit BMP and 16-bit PNG export helpers.

### Spatial Sampling and Partitioning

- Poisson disk point sampling with constant or spatially varying minimal distance.
- Weighted Voronoi partitioning for positioned items.

### Influence Fields

- Influence fields for sampling values from point or curve sources.
- Source culling and interpolation strategies for local field behavior.
- Heat map rasterization for floating-point influence fields.

## Documentation

Documentation is available at:

[https://akeldov.github.io/Akeldov.Math/Spatial2D/](https://akeldov.github.io/Akeldov.Math/Spatial2D/)
