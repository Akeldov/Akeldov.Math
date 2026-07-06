# Akeldov.Math.Hexes

Akeldov.Math.Hexes is a .NET library for hex-grid coordinate systems, topology, geometry, rasterization, polyhex contours, and partitioning utilities.

## Features

- [Vectors](vectors.md)
    - [Types](vectors/types.md)
    - [Coordinate Conversions](vectors/coordinate-conversions.md)
    - [Discretization](vectors/discretization.md)
    - [Transformations](vectors/transformations.md)
    - [Serialization](vectors/serialization.md)
- [Layouts](layouts.md)
- [Topology](topology.md)
    - [Edges and Vertices](topology/edges-and-vertices.md)
    - [Adjacency](topology/adjacency.md)
    - [Topology Maps](topology/topology-maps.md)
    - [Topology Grids](topology/topology-grids.md)
    - [Polyhex Topology](topology/polyhex-topology.md)
- [Geometry](geometry.md)
    - [Hex Geometry](geometry/hex-geometry.md)
    - [Geometry Maps](geometry/maps.md)
    - [Geometry Grids](geometry/grids.md)
    - [Polyhex Geometry](geometry/polyhex-geometry.md)
    - [Field Parameters](geometry/field-parameters.md)
    - [Partitioning](geometry/partitioning.md)
- [Maps](maps.md)
    - [IHexMap](maps/ihexmap.md)
    - [HexMap](maps/hexmap.md)
    - [Shared Map Behavior](maps/shared-map-behavior.md)
- [Grids](grids.md)
    - [IGrid](grids/igrid.md)
    - [Shared Grid Behavior](grids/shared-grid-behavior.md)
    - [Rasterization Integration](grids/rasterization-integration.md)
- [Chromatization](chromatization.md)
    - [Chromatic Indexes](chromatization/chromatic-indexes.md)
    - [Chromatic Maps](chromatization/chromatic-maps.md)
    - [Chromatic Grids](chromatization/chromatic-grids.md)
    - [Rasterization](chromatization/rasterization.md)
- [Utility Structures](utility-structures.md)
    - [Generic Containers](utility-structures/generic-containers.md)
    - [Presence Flags](utility-structures/presence-flags.md)
    - [Sixfold Angles](utility-structures/sixfold-angles.md)
    - [Binary Helpers](utility-structures/binary-helpers.md)

## Installation

```powershell
dotnet add package Akeldov.Math.Hexes --version 0.1.0
```

## Target Frameworks

- .NET Standard 2.1
- .NET 6.0

## Related Libraries

Akeldov.Math.Hexes builds on [Akeldov.Math.Spatial2D](../Spatial2D/index.md) for point, vector, contour, region, raster, and Voronoi primitives.
