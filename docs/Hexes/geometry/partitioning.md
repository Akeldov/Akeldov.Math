# Partitioning

Partitioning assigns hex centers to weighted Voronoi sites.

## Voronoi Cells

- `VoronoiCell` stores the site index and assigned hex indexes.
- Empty cells are preserved by default when a site receives no hexes.
- Cell inputs are validated before construction.

## Partition Maps

- `VoronoiHexPartitionMap` stores Voronoi cells in a hex map.
- `MaskedVoronoiHexPartitionMap` stores nullable assignments for a masked partition.
- The map preserves layout and index metadata.
- Hex centers provide the sampled point set for partitioning.
- Cell assignments are read-only on the partition result, so they remain consistent with `Cells`.
- `Cells` is a read-only semantic result in source-site order, with empty cells handled by the
  selected policy. `SiteIndex` matches the cell's index in this result, without gaps.
- Use `ToMutableHexMap()` to create a mutable caller-owned copy of the per-hex assignments.

## Empty Cells

Pass `EmptyCellPolicy` from `Akeldov.Math.Spatial2D.Partitioning.Voronoi` to
`VoronoiHexPartitioner` or `ToVoronoiHexPartitionMap`:

- `LeaveAsIs` preserves empty cells. Calls without a policy keep this behavior.
- `Exclude` removes empty cells and renumbers the remaining `SiteIndex` values from zero.
  Per-hex assignments reference the corresponding cells in the compacted `Cells` list.
- `ThrowException` throws `InvalidOperationException` if any site receives no participating hexes.

```csharp
var partition = hexCenters.ToVoronoiHexPartitionMap(sites, EmptyCellPolicy.Exclude);
var maskedPartition = hexCenters.ToVoronoiHexPartitionMap(
    sites, participationMask, EmptyCellPolicy.Exclude);
```

## Participation Masks

- These overloads accept an `IHexMap<bool>` participation mask.
- `Partition(hexCenters, participationMask)` and
  `ToVoronoiHexPartitionMap(sites, participationMask)` assign only hexes whose mask value is
  `true`.
- The participation mask must have the same topology as the center map.
- Excluded hexes return `null` from `MaskedVoronoiHexPartitionMap`; they are not included in any
  cell's `HexIndexes`.

## Region Masks

Pass an `IHexMap<int>` to restrict each hex to sites in the same region:

```csharp
var regionPartition = hexCenters.ToVoronoiHexPartitionMap(sites, regionMask);
var compactRegionPartition = hexCenters.ToVoronoiHexPartitionMap(
    sites, regionMask, EmptyCellPolicy.Exclude);
```

Equal mask values identify the same region, including disconnected hexes. Zero and negative
values are valid region identifiers; every hex participates. The mask must have the same
topology as the center map. A site's region comes from the hex containing its position,
using the center map's geometry. Sites outside the map receive no hexes and follow the
empty-cell policy. A hex without an eligible site in its region causes an
`InvalidOperationException`. The result is a `VoronoiHexPartitionMap`.

## Weighted Sites

- Larger weights can pull farther centers into a cell.
- Zero-weight sites only receive exact site points.
- Infinite-weight sites are handled as a special nearest-site case.
