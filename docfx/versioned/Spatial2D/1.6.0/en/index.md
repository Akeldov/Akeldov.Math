# Akeldov.Math.Spatial2D

Spatial2D provides two-dimensional geometry, curves, contours, regions, rasterization,
spatial sampling, partitioning, and influence fields.

## Install

```powershell
dotnet add package Akeldov.Math.Spatial2D --version 1.6.0
```

## New in 1.6.0

- Use `bounds.PartitionByVoronoi(sites)` to split a rectangle into regions. Equal weights produce straight boundaries. Different positive weights can produce circular arcs. A cell can have holes or several components.
- Call `GetCentroid()` on a cell with area, or pass `relaxationIterations` to move sites toward their area centroids.
- Generate a fixed number of independent points with `RandomPointSampler`, or use `SamplePoissonDiskSites` to set minimum spacing from a field in world coordinates.
- Fill regions on spatial RGBA rasters and color pixels by distance. Convert between generic and specialized Boolean, integer, and floating-point spatial rasters in either direction.

The package targets .NET Standard 2.1 and .NET 6. The `VoronoiItemPartition<TItem>` constructor still copies the supplied items. Use `FromList` when later edits to the supplied list should appear in the partition.

Learn how to [build Voronoi regions and draw their contours](concepts/partitioning/voronoi-regions.md) or [group positioned items by site](concepts/partitioning/voronoi.md).

[View the package on NuGet](https://www.nuget.org/packages/Akeldov.Math.Spatial2D/)
