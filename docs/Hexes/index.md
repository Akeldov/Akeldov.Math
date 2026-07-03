# Akeldov.Math.Hexes

Akeldov.Math.Hexes is a .NET library for hex-grid coordinate systems, topology, geometry, rasterization, polyhex contours, and partitioning utilities.

## Features

- QRS axial vector types and conversion helpers for row- and column-oriented hex layouts.
- Odd/even row and column layout helpers for centers, vertices, adjacency, and bounding boxes.
- Hex maps and grids for topology, barycentric sampling, chromatic indexes, and rasterization.
- Polyhex masks, contour generation, apothem offsets, and region conversion helpers.
- Weighted Voronoi partitioning over hex center maps.

## Installation

```powershell
dotnet add package Akeldov.Math.Hexes --version 0.1.0
```

## Target Frameworks

- .NET Standard 2.1
- .NET 6.0

## Related Libraries

Akeldov.Math.Hexes builds on [Akeldov.Math.Spatial2D](../Spatial2D/index.md) for point, vector, contour, region, raster, and Voronoi primitives.
