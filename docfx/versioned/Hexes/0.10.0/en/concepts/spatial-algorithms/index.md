# Spatial Algorithms

The spatial algorithms in Akeldov.Math.Hexes solve problems over an existing hex grid. They do
not mutate their source maps: an algorithm reads topology, geometry, or cell values and returns a
separate result.

The main distinction is which properties of the grid affect that result:

| Task | Required data | What determines the result | Result |
|---|---|---|---|
| Find a route | Topology and entry- and exit-cost maps | Adjacency and accumulated transition cost | <xref:Akeldov.Math.Hexes.Pathfinding.HexPath> or `null` |
| Partition a map among sites | Map geometry and weighted Spatial2D sites | World-space hex centers, site positions, and site weights | `SpatialHexPartitionMap<VoronoiHexPartitionCell>` |
| Obtain stable hex classes | Index and layout | Position in the logical hex lattice | Class `0`, `1`, or `2` |
| Partition by area shares | Geometry, noise settings, targets, optional regions and mask | Noise bands and relative target shares | Full or partial spatial partition |
| Evolve territory boundaries | Partition, energy settings, random generator, optional regions | Neighbor-copy attempts, boundary energy, and area penalties | New partition with preserved cell IDs |
| Split disconnected territories | Full or partial partition | Six-neighbor connectivity within each cell | One new cell per connected component |
| Expand, shrink, or outline a mask | Boolean map | Cell values and six edge neighbors | Boolean map |
| Find regions or distances in a mask | Boolean map, seed or target value | Six-neighbor connectivity and hex-step distance | Region mask, component labels, or distance map |
| Generate or smooth a numeric field | Topology or geometry and noise settings; float map for smoothing | Perlin noise or a Gaussian neighborhood kernel | Floating-point map |

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
how to work with the partition result.

## Chromatization

Chromatization defines a proper three-coloring of the hex lattice: cells that share an edge always
belong to different classes. Classes are derived without inspecting map values and can be used
directly, stored in <xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>, or used to give
raster weights a stable order.

Continue to [Chromatization](chromatization.md) to distinguish `Main`, `Left`, `Right` order from
chromatic `Index0`, `Index1`, `Index2` order.

## Perlin Noise Partitioning

`PartitionByPerlinNoise` divides noise sampled at world-space hex centers into ordered bands.
Targets specify unique non-negative IDs and relative area shares. Shares are approximate: equal
noise values stay together. Region overloads compute thresholds separately in each region;
partial overloads leave excluded hexes unassigned. One territory can contain disconnected pieces.

See [Partition and Diffuse Territories](../../how-to-guides/geometry-and-polyhexes/partition-and-diffuse-territories.md)
for targets, seeds, participation masks, and regional constraints.

## Cellular Potts Diffusion

`DiffuseByCellularPotts` evolves an existing partition through stochastic neighbor-copy attempts.
Boundary energy favors shorter interfaces; area penalties favor target hex counts. These are soft
preferences, not guarantees of exact areas or a minimum-energy result. Supply a seeded `Random`
for repeatable runs.

The default `Preserve` connectivity policy requires initially connected non-empty cells and
prevents fragmentation or disappearance. `None` permits both. Region boundaries restrict copying;
partial-map nulls never participate. The result preserves cell IDs and order.

See [Partition and Diffuse Territories](../../how-to-guides/geometry-and-polyhexes/partition-and-diffuse-territories.md)
for connectivity policies and energy settings.

## Splitting Disconnected Cells

`SplitDisconnectedCells` creates one partition cell for each six-connected component of an
existing territory. It assigns new IDs from zero in row-major discovery order, drops empty cells
and specialized cell metadata, and preserves unassigned hexes in partial maps.

Use it before connectivity-preserving diffusion when disconnected pieces should become
independent territories. Spatial overloads retain geometry; keep the receiver's spatial static
type to select them. See [Space Partitioning](space-partitioning.md).

## Boolean Morphology

`Dilate`, `Erode`, `Open`, `Close`, and `Outline` process masks using a hex and its six
edge neighbors. Missing neighbors beyond the finite topology are ignored. Opening applies
erosion followed by dilation; closing applies dilation followed by erosion.

Use these operations to expand or contract selected areas, remove small features, or extract
their outline. See [Maps](../data-storage/maps.md#process-boolean-regions) for examples.

## Connectivity and Distance Transforms

`FloodFill` returns the six-connected region with the same Boolean value as the seed, including
a `false` region when the seed is false. `ConnectedComponents` labels `true` components with
positive integers and leaves false cells at zero. `DistanceTransform` returns the minimum number
of hex steps to the requested Boolean value, or `int.MaxValue` when that value is absent.

These operations use unweighted adjacency. Use pathfinding when movement costs affect distance,
and `SplitDisconnectedCells` when the input is a territory partition rather than a Boolean mask.
See [Maps](../data-storage/maps.md#process-boolean-regions).

## Noise Generation and Smoothing

`CreatePerlinNoise` generates a floating-point field; `GaussianBlur` smooths an existing field
and normalizes its kernel at map boundaries. Topology-based noise uses unit-radius hex centers;
geometry-based noise uses world-space centers. Choose the source according to the coordinate
system in which the noise scale should be measured.

Unlike `PartitionByPerlinNoise`, generation returns numeric values rather than territory IDs.
See [Generate and Smooth Noise](../../how-to-guides/maps-and-topology/generate-and-smooth-noise.md).

## Choosing an Algorithm

- Use pathfinding when the answer depends on a chain of adjacent transitions.
- Use Voronoi partitioning when the answer depends on world-space distance.
- Use chromatization when you need a repeatable class shared by the entire infinite lattice.
- Use noise partitioning for territories with approximate area shares.
- Use Cellular Potts diffusion to evolve boundaries with area and connectivity preferences.
- Split disconnected cells when each connected piece should be an independent territory.
- Use morphology to change a mask's shape, and connectivity methods to inspect its regions or distances.
- Generate or blur noise when the desired result is a numeric field.

The maps, rasters, and complete or partial neighborhoods used as inputs and results are introduced
in [Data Storage](../data-storage/index.md).
