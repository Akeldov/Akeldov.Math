# Procedural Space Partitioning

In this tutorial, you will generate well-spaced sites and use them to divide a rectangular map
into weighted Voronoi regions. You will then relax the sites toward their cell centroids and
export the result as an SVG image.

The partition is discrete: Spatial2D assigns positioned map cells to sites. Increasing the grid
resolution produces a finer approximation of continuous Voronoi boundaries.

## What You Will Build

You will:

1. generate deterministic Poisson disk points;
2. turn those points into Voronoi sites;
3. partition a regular grid of map cells;
4. give selected sites more influence;
5. apply centroid relaxation;
6. render the resulting partitions and sites.

Each page continues the same console application. The final page contains the complete source.

## Create the Project

Install the .NET 6 SDK or later, then run:

```powershell
dotnet new console --name Spatial2D.Partitioning
Set-Location Spatial2D.Partitioning
dotnet add package Akeldov.Math.Spatial2D --version 1.1.0
```

Start with an empty `Program.cs`, then continue with
[Generating Poisson Points](generating-poisson-points.md).
