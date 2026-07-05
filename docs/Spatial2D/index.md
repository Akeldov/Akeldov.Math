# Akeldov.Math.Spatial2D

Akeldov.Math.Spatial2D is a .NET library for practical two-dimensional geometry, contours, regions, rasterization, spatial sampling, partitioning, and value fields.

## Features

The library is organized around a few practical geometry workflows.

### Core Geometry

- Point and vector types: `PointXY`, `VectorXY`, and `VectorXYInt`.
- Curve primitives: `Line`, `Ray`, `Segment`, and `Arc`.
- Projection, distance, intersection, angle, and centroid helpers.

### Boundaries and Areas

- Closed contours made from bounded parameterized curves, including circular and rectangular contours.
- Filled regions with holes and nested contours.
- Contour smoothing and corner filleting helpers.

### Rasterization and Imaging

- Axis-aligned raster grids for sampling geometry into cells.
- Signed-distance rasterizers for contours and regions.
- Mutable spatial and non-spatial grayscale and RGBA rasters with image export helpers.

### Spatial Sampling

- Poisson disk point sampling with constant or spatially varying minimal distance.

### Spatial Partitioning

- Weighted Voronoi partitioning for positioned items.

### Influence Fields

- Value fields sampled from point or curve sources.
- Source culling and interpolation strategies for local field behavior.

Most primitives are immutable, and public collection inputs are copied where mutation would otherwise leak into library state. Geometry comparisons use the shared `GeometryConstants.GeometryEpsilon` tolerance where exact floating-point equality would be too brittle.

## Installation

```powershell
dotnet add package Akeldov.Math.Spatial2D
```

## Quick Example

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var segment = new ParameterizedSegment(
    new PointXY(0f, 0f),
    new PointXY(10f, 0f));

var projection = segment.ProjectWithParameter(new PointXY(4f, 3f));

PointXY closestPoint = projection.ProjectedPoint;
float curveCoordinate = projection.CurveCoordinate;
float distance = projection.Distance;
```

## Target Frameworks

- .NET Standard 2.1
- .NET 6.0

## Documentation Map

- [Concepts](concepts/index.md) covers shared conventions such as coordinate modeling, angle units, tolerance, collection ownership, and scaling.
- [Points](points.md), [Vectors](vectors.md), and [Curves](curves/index.md) cover the core spatial primitives.
- [Contours](contours/index.md) and [Regions](regions/index.md) cover boundaries and filled areas.
- [Fields](fields/index.md), [Sampling](sampling/index.md), and [Partitioning](partitioning/index.md) cover higher-level spatial workflows.
- [Rasterization and Imaging](rasterization/index.md) covers grids, rasters, signed distance, heatmaps, and image export.
- [Recipes](recipes/index.md) shows common end-to-end tasks.
- [Performance](performance/index.md) and [Testing and Robustness](testing/index.md) describe verification and benchmark coverage.
