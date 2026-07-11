# Akeldov.Math.Spatial2D

Akeldov.Math.Spatial2D is a .NET library for practical two-dimensional geometry, vector math, contours, regions, rasterization, spatial sampling, partitioning, and influence field utilities.

## Features

The library is organized around practical 2D geometry workflows.

### Core Geometry

- Point and vector types: `PointXY`, `VectorXY`, and `VectorXYInt`.
- Vector math helpers for distance, transformations, rounding, scaling, and serialization.
- Linear and circular curve primitives, including parameterized lines, segments, arcs, and paths.
- Quadratic, cubic, and arbitrary-degree Bézier curves.
- Projection, distance, intersection, angle, contour, and centroid helpers.

### Boundaries and Areas

- Closed contours made from bounded parameterized curves, including circular and rectangular contours.
- Filled regions with holes and nested contours.
- Axis-aligned, oriented, and normalized rectangle primitives.
- Contour smoothing and corner filleting helpers.
- Signed point-distance contracts for contours and regions.

### Rasterization and Imaging

- Generic grids and axis-aligned spatial raster grids with bounds- and pixel-density-based construction.
- Mutable non-spatial `Raster<T>` and world-space `SpatialRaster<T>` data.
- Distance, signed-distance, and stroke rasterizers for curves, contours, regions, and collections.
- 8-bit and 16-bit grayscale and RGBA color types.
- Color helpers for normalized values, blending, and temperature heat maps.
- Generic geometry scenes with composable shape and TrueType text layers.
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
