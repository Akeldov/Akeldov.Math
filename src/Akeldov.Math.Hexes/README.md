# Akeldov.Math.Hexes

Akeldov.Math.Hexes is a .NET library for hex-grid coordinates, topology, geometry, maps, rasterization, pathfinding, polyhex processing, and weighted partitioning.

## Features

The library is organized around practical hex-grid workflows.

### Coordinates and Layouts

- Fractional and integer QRS axial vectors with the cube-coordinate zero-sum invariant.
- Conversion between QRS, offset-grid indexes, and two-dimensional world coordinates.
- Odd and even row layouts for pointy-top hexes and odd and even column layouts for flat-top hexes.
- Layout-aware discretization, rotation, affine transformations, and binary serialization.
- Sixfold angles for directions and rotations in 60-degree increments.

### Topology and Maps

- Rectangular map topology with edge, vertex, pair, triplet, and six-neighbor adjacency helpers.
- Mutable `HexMap<T>` and geometry-aware `SpatialHexMap<T>` value storage.
- Versioned binary stream and file serialization for Boolean, integer, and floating-point maps, including spatial geometry and exact floating-point bits.
- Dedicated Boolean, integer, and floating-point maps in topology-only and geometry-preserving spatial variants, with independent copy methods and constant-time shared-storage spatial conversions.
- Generic and specialized value mapping from individual cells or partial six-neighbor sets, with spatial geometry preserved when present.
- Element-wise, scalar, mixed numeric, and spatial/non-spatial arithmetic, remainder, and comparison-mask operators, including operands typed as `HexMap<int>` and `HexMap<float>`, plus clamping and range rescaling.
- Sampling of Spatial2D integer and floating-point fields at hex centers, including randomized pointwise ranges.
- Logical operations, multi-ring and per-cell distance-limited morphology, outlines, flood fill, connected components, and distance transforms for Boolean maps.
- Deterministic Perlin-noise generation and Gaussian blur for floating-point maps.
- Index maps and compact pair, triplet, sextuplet, and septuplet containers with presence flags, including layout-aware six-neighbor sampling.
- Polyhex masks, builders, extension operations, and binary serialization.

### Geometry and Rasterization

- Radius-based hex geometry with layout-aware centers, vertices, bounding boxes, and raster geometry.
- Barycentric sampling for the three hexes meeting at a vertex.
- Topology and geometry rasters with arbitrary world-space bounds and resolution.
- Generic map rasterization and optional XY or QRS index labels.
- Polyhex boundary extraction, contour construction, apothem offsets, and region conversion.

### Chromatization

- Repeating chromatic indexes for hexes and shared vertices.
- Chromatic maps and full or partial triplet rasters.
- Barycentric chromatic weights for blending values around hex vertices.

### Pathfinding and Partitioning

- Weighted shortest-path search using separate cell entry and exit costs.
- Impassable cells represented by positive-infinity transfer costs.
- Generic full and partial partition maps with mutable cell identifiers and read-only cell groups.
- Weighted Voronoi partitioning with participation and nullable region masks, configurable empty-cell handling, and disconnected-component reassignment.

## Documentation

Documentation is available at:

[https://akeldov.github.io/Akeldov.Math/Hexes/](https://akeldov.github.io/Akeldov.Math/Hexes/)
