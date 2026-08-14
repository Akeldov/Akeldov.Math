# Spatial Algorithms

Akeldov.Math.Hexes provides algorithms that use the structure of a hex grid to find routes,
partition a map, and classify cells consistently. They share the same index and layout model,
but solve different problems and require different inputs.

| Task | What determines the result | Primary result |
|---|---|---|
| Find a minimum-cost route | Topology and entry and exit costs | <xref:Akeldov.Math.Hexes.Pathfinding.HexPath> |
| Assign hexes to the nearest weighted sites | Spatial hex centers and Voronoi sites | <xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitionMap> |
| Separate adjacent hexes into stable classes | Hex index and layout | Class `0`, `1`, or `2` |

Pathfinding follows logical adjacency within <xref:Akeldov.Math.Hexes.HexMapTopology>, while
chromatization only needs an index and layout. Voronoi partitioning compares center positions in
continuous space, so it also requires map geometry.

## Pathfinding

Pathfinding treats the map as a directed, weighted graph. Each hex is a vertex, and transitions
are allowed between the six edge-adjacent neighbors. The topology layout determines which indices
are adjacent in each row or column.

<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap> combines two maps with matching
topologies. The cost of one step from `from` to `to` is the exit cost of `from` plus the entry
cost of `to`. A reverse transition can therefore have a different cost even though the same two
cells remain adjacent.

`FindShortestPath` uses Dijkstra's algorithm and returns the path with the lowest total cost. All
finite costs must be non-negative, and `float.PositiveInfinity` marks an impassable entry or
exit. The resulting <xref:Akeldov.Math.Hexes.Pathfinding.HexPath> contains the indices from the
source hex through the destination and the sum of every step cost. The method returns `null` when
the destination is unreachable.

See [Pathfinding](pathfinding.md) for the cost model, constraints, and search result.

## Space Partitioning

Voronoi partitioning assigns the center of each hex to one weighted Spatial2D site. Unlike
pathfinding, it does not traverse grid edges: the algorithm compares positions from
<xref:Akeldov.Math.Hexes.Geometry.HexCenterMap> with site positions and weights in continuous
space. Increasing a site's weight lets it attract centers from farther away.

<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitioner> performs the assignment, and
<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitionMap> stores the consistent,
read-only result. Its indexer returns the Voronoi cell assigned to a specific hex, while `Cells`
contains one <xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiCell> for every source site,
including sites with no assigned hexes.

The result retains the topology and geometry of the source center map. If assignments must be
changed independently, `ToMutableHexMap()` creates a new mutable map without affecting the
consistency of the original result.

See [Space Partitioning](space-partitioning.md) for weight behavior, cell contents, and the result
model.

## Chromatization

Chromatization defines a proper three-coloring of the infinite hex lattice. Each index receives
class `0`, `1`, or `2`, and any two hexes that share an edge belong to different classes. The
class is derived deterministically from the index and layout; map values, radius, and origin do
not affect it.

Use `GetChromaticClass(layout)` for one index, or
<xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap> to precompute classes for a whole map
while retaining its geometry for later spatial sampling. For each sample point, chromatic rasters
find the nearest grid vertex and store either the class numbers or the class-ordered barycentric
weights of the three hexes incident to it. Partial variants also mark hexes that are absent at the
boundary of a finite map.

This classification is useful for independent passes over nonadjacent cells and for stable value
ordering during interpolation and rasterization. See [Chromatization](chromatization.md) for the
details.

## Choosing an Algorithm

- Use pathfinding when the result must account for map connectivity, obstacles, and the
  accumulated cost of consecutive transitions.
- Use Voronoi partitioning when each cell must be assigned to a spatial site according to its
  position and weight.
- Use chromatization when each cell needs a repeatable class that differs from every edge-adjacent
  neighbor.

These algorithms read topology, geometry, or maps introduced in [Data Storage](../data-storage/index.md).
Continue to [Rasterization](../rasterization.md) for regularly spaced spatial sampling of hex maps
and map-like results.
