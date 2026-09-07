# Item Partitioning

Partitioning algorithms group existing objects according to their positions. They determine
item membership rather than generate new points.

## Available algorithms

- [Voronoi Item Partitioning](voronoi.md) — assigning objects to weighted sites, handling empty
  partitions, and relaxing sites toward the centroids of their assigned items.

The result contains groups of objects, not polygonal Voronoi cell boundaries. To generate
initial site positions first, use [Samplers](../samplers/index.md).
