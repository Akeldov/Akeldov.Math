# Akeldov.Math.Hexes

Akeldov.Math.Hexes provides hex-grid coordinates, layouts, topology, geometry, maps, pathfinding,
rasterization, polyhex processing, and weighted partitioning.

## Install

```powershell
dotnet add package Akeldov.Math.Hexes --version 0.10.0
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

## New in 0.10.0

Spatial partition maps retain geometry through connected-component splitting and Cellular Potts
diffusion. Voronoi and Perlin results use the common spatial partition types. `IHexMap<T>` is
covariant, and `ToOffsetRegion(distance, layout)` expands polyhex regions by a configurable distance.

See [migration from 0.9.0](how-to-guides/geometry-and-polyhexes/migrate-from-090.md)
for breaking API changes and replacement types.
