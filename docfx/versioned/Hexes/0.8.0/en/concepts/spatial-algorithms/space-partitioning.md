# Space Partitioning

Akeldov.Math.Hexes applies the weighted Spatial2D Voronoi model to a finite hex map. Each whole
hex is assigned to one site according to the world-space position of its center. The result is
useful for territories, influence zones, and distributing cells among control points.

## A Discrete Center Partition

<xref:Akeldov.Math.Hexes.Geometry.HexCenterMap> supplies the source points. It derives every hex
center from <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>: topology, origin, and radius.

The algorithm compares only these centers. It does not cut hexes along a Voronoi boundary or
return vector polygons. If a continuous boundary crosses a cell, the entire cell is still
assigned to the site nearest to its center under the weighted metric.

This has two important consequences:

- equal topologies with different origins or radii can produce different assignments;
- a site may lie outside the map geometry and still receive nearby hexes.

## Weighted Sites

`Site` from `Akeldov.Math.Spatial2D.Partitioning.Voronoi` contains a world-space `Position` and a
non-negative `Weight`. For an ordinary finite positive weight, the algorithm compares:

```text
weightedDistance² = distance(center, site.Position)² / site.Weight²
```

A larger weight reduces the weighted distance and expands the site's influence. Weight is not an
additive cost; it scales distance from the site position.

Zero and infinite weights have special meanings:

| Weight | Behavior |
|---|---|
| Finite and positive | Participates in ordinary weighted-distance comparison |
| `0` | Receives only a center coincident with the site position within geometry tolerance |
| `float.PositiveInfinity` | Takes precedence over finite weights for non-coincident points; the nearest infinite-weight site wins among infinite sites |

The site list cannot be empty, and at least one site must have nonzero weight. Positions must be
finite, weights must be non-negative and not `NaN`, and calculated hex-center coordinates must
also remain finite.

## Partition Maps and Identifiers

`centers.ToVoronoiHexPartitionMap(sites)` returns a `VoronoiHexPartitionMap` derived from
`HexPartitionMap<VoronoiHexPartitionCell>` and `IntHexMap`. Its indexer returns a cell identifier:

```csharp
int cellId = partition[index];
VoronoiHexPartitionCell cell = partition.Cells[cellId];
```

Partial overloads are named `ToPartialVoronoiHexPartitionMap`. Their result inherits
`PartialHexPartitionMap<VoronoiHexPartitionCell>` and `HexMap<int?>`; excluded hexes initially
contain `null`. A Boolean mask selects participating hexes. A nullable integer region mask combines
participation and region labels: `null` excludes a hex, while every integer is a valid region ID.
With region masks, only sites in the same region are eligible; a participating hex without an
eligible site causes `InvalidOperationException`.

`Cells` is a read-only structural result. Each `VoronoiHexPartitionCell` exposes `Id`, `SiteIndex`,
`Site`, `Center`, and read-only `HexIndexes`. Initial identifiers equal `SiteIndex` and index
`Cells`. `EmptyCellPolicy.LeaveAsIs` retains empty cells; `Exclude` removes them and compacts
identifiers in source-site order; `ThrowException` rejects an empty cell. After compaction,
`SiteIndex` is no longer an index into the original site list.

The public entry points are the extensions on `HexCenterMap`. `VoronoiHexPartitioner` is internal.
`HexCenterMap` initially derives its values from geometry and now inherits mutable `HexMap<PointXY>`.
Partitioning uses the stored center values; editing them does not change the retained geometry.

## Disconnected Components

Masked or weighted assignments can split a cell into several six-connected components.
`ExclavePolicy.LeaveAsIs` preserves them. `ReassignToClosestCell` retains the component containing
the cell's closest hex to its site and grows from retained components in simultaneous layers.
Each reassigned hex selects an adjacent grown cell by unweighted Euclidean distance to its site;
ties favor the earlier source site. Excluded hexes and region boundaries are preserved.

A component unreachable from retained components becomes a new cell after the source cells.
Its site is the component hex closest to the original site, with the original weight; row-major
order resolves ties. Empty-cell handling is applied after reassignment.

## Mutable Assignments and Shared Facades

Partition indexers are mutable. A write changes only the identifier map; it does not rebuild
`Cells`, their `HexIndexes`, or the original participation mask. Treat the retained groups as
the initial algorithm result, and maintain application-specific groups separately after editing.

Implicit conversion to `SpatialHexMap<int>` or `SpatialHexMap<int?>` creates a new facade with
the same geometry and shared assignment storage in O(1). Writes through either facade are visible
through the other. `ToMutableHexMap()` instead returns a new independent caller-owned assignment
map. `ToMutableParticipationMask()` copies the original partial-partition mask.

See [Partition a Map by Nearest Sites](../../how-to-guides/geometry-and-polyhexes/partition-a-map-by-nearest-sites.md)
for complete examples, masks, and migration from 0.7.0.
