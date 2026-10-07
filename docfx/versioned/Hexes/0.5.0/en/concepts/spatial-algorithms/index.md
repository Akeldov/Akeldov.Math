# Spatial Algorithms

The spatial algorithms in Akeldov.Math.Hexes solve problems over an existing hex grid. They do
not mutate their source maps: an algorithm reads topology, geometry, or cell values and returns a
separate result.

The main distinction is which properties of the grid affect that result:

| Task | Required data | What determines the result | Result |
|---|---|---|---|
| Find a route | Topology and entry- and exit-cost maps | Adjacency and accumulated transition cost | <xref:Akeldov.Math.Hexes.Pathfinding.HexPath> or `null` |
| Partition a map among sites | Map geometry and weighted Spatial2D sites | World-space hex centers, site positions, and site weights | <xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitionMap> |
| Obtain stable hex classes | Index and layout | Position in the logical hex lattice | Class `0`, `1`, or `2` |

## Topology and Geometry

<xref:Akeldov.Math.Hexes.HexMapTopology> defines the finite set of cells and their adjacency. That
is enough for pathfinding: hex radius and world-space origin do not change the available
transitions. The chromatic class of one index needs even less information—the index itself and
<xref:Akeldov.Math.Hexes.Layout>.

Voronoi partitioning answers a spatial question, so it uses
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>.
<xref:Akeldov.Math.Hexes.Geometry.HexCenterMap> derives the world-space position of every cell
from that geometry. Changing the origin, scale, or layout can change assignments even when the
map resolution stays the same.

## Pathfinding

The map is treated as a directed graph: hexes become vertices and shared edges become allowed
transitions. <xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap> defines one step as the exit
cost of the source hex plus the entry cost of the next hex. `FindShortestPath` uses Dijkstra's
algorithm and returns a route with the minimum total cost.

Continue to [Pathfinding](pathfinding.md) to configure directed costs, obstacles, and unreachable
destinations.

## Space Partitioning

Weighted Voronoi partitioning assigns every hex center to one Spatial2D site. This is a discrete
partition: the algorithm classifies whole hexes by their centers and does not construct vector
boundaries inside cells. The result is available both as a per-index assignment and as the list
of hexes belonging to each Voronoi cell.

Continue to [Space Partitioning](space-partitioning.md) to learn how weights affect assignment and
how to work with the immutable result.

## Chromatization

Chromatization defines a proper three-coloring of the hex lattice: cells that share an edge always
belong to different classes. Classes are derived without inspecting map values and can be used
directly, stored in <xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>, or used to give
raster weights a stable order.

Continue to [Chromatization](chromatization.md) to distinguish `Main`, `Left`, `Right` order from
chromatic `Index0`, `Index1`, `Index2` order.

## Choosing an Algorithm

- Use pathfinding when the answer depends on a chain of adjacent transitions.
- Use Voronoi partitioning when the answer depends on world-space distance.
- Use chromatization when you need a repeatable class shared by the entire infinite lattice.

The maps, rasters, and complete or partial neighborhoods used as inputs and results are introduced
in [Data Storage](../data-storage/index.md).
