# Akeldov.Math.Hexes

Akeldov.Math.Hexes provides hex-grid coordinates, layouts, topology, geometry, maps, pathfinding,
rasterization, polyhex processing, and weighted partitioning.

## Install

```powershell
dotnet add package Akeldov.Math.Hexes --version 0.9.0
```

## Highlights

- Fractional and integer QRS coordinates
- Pointy-top and flat-top layouts
- Boolean, integer, and floating-point maps with topology-only and spatial variants
- Cell-wise arithmetic, comparisons, range transforms, and Boolean morphology
- Layout-aware six-neighbor sampling, flood fill, component labeling, and distance transforms
- Perlin-noise generation and Gaussian blur
- Weighted pathfinding, rasterization, chromatization, and polyhex geometry
- [Binary stream and file serialization](how-to-guides/maps-and-topology/save-and-load-hex-maps.md) for all six map types

[View the package on NuGet](https://www.nuget.org/packages/Akeldov.Math.Hexes/)

## New in 0.9.0

Perlin-noise partitions with target area shares, Cellular Potts diffusion with optional region
constraints, connected-component splitting, and configurable Voronoi relaxation.

See [partition and diffusion examples](how-to-guides/geometry-and-polyhexes/partition-and-diffuse-territories.md)
and [migration from 0.8.0](how-to-guides/geometry-and-polyhexes/partition-a-map-by-nearest-sites.md#migrate-from-080).
