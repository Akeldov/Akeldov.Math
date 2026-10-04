# Partitioning

Partitions group hex indexes. Voronoi partitioning assigns hex centers to weighted sites.

## Partition Cells and Partitions

`IHexPartitionCell` in `Akeldov.Math.Hexes` represents one part of a partition and exposes the
non-negative `Id` and read-only `HexIndexes` sequence. `HexPartitionCell` accepts the ID in its
constructor and implements this contract by copying the
constructor input into a read-only snapshot.
Later changes to the input collection do not affect the cell, and the returned collection
cannot be modified through a mutable collection interface. Empty cells are allowed;
input order and duplicate indexes are preserved. Index bounds and map membership are not validated.

`VoronoiCell` derives from `HexPartitionCell`, so code that needs only the assigned hex indexes
can accept `IHexPartitionCell` for both manually constructed cells and Voronoi cells.

`IHexPartition` inherits `IHexMap<int>`: both map indexers return the assigned cell's `Id`.
Every hex in its `Topology` belongs to exactly one cell. IDs are unique within a partition;
they need not be consecutive or match positions in its read-only `Cells` collection.

`HexPartition` accepts an explicit `HexMapTopology` and copies the supplied collection,
retaining the cell objects in their original order. It validates identifiers, index bounds,
overlap between different cells, and complete map coverage. Empty cells are allowed;
an empty cell collection is valid only for an empty topology. Null collections and null cells
are rejected. Custom `IHexPartitionCell` implementations must keep their IDs and indexes stable
to remain consistent with the stored assignment snapshot.

`IPartialHexPartition` inherits `IHexMap<int?>` and exposes the same read-only `Cells` contract.
Assigned hexes contain their cell IDs; unassigned hexes contain `null`. Each assigned hex belongs
to exactly one cell, while complete map coverage is not required.

```csharp
var topology = new HexMapTopology(2, 1, Layout.OddR);
IHexPartitionCell cell = new HexPartitionCell(42,
    new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) });
IHexPartition partition = new HexPartition(topology, new[] { cell });
int cellId = partition[new VectorXYInt(1, 0)]; // 42
IReadOnlyList<VectorXYInt> indexes = partition.Cells[0].HexIndexes;
```

## Voronoi Cells

- `VoronoiCell` stores the site index and assigned hex indexes. Its `Id` equals `SiteIndex`.
- Empty cells are preserved by default when a site receives no hexes.
- Cell inputs are validated before construction.

## Partition Maps

- `VoronoiHexPartitionMap` stores Voronoi cell IDs in a spatial hex map.
- `MaskedVoronoiHexPartitionMap` implements `IPartialHexPartition` and `ISpatialHexMap<int?>`.
  Its indexers return nullable cell IDs: `null` identifies an excluded hex.
- `VoronoiHexPartitionMap` implements `IHexPartition` and `ISpatialHexMap<int>`.
  Both public indexers return the assigned cell's ID. Access the corresponding `VoronoiCell`
  through `partition.Cells[partition[index]]`. `Cells` remains an `IReadOnlyList<VoronoiCell>`.
- `MaskedVoronoiHexPartitionMap` follows the partial partition contract, allowing excluded hexes.
- The map preserves layout and index metadata.
- Hex centers provide the sampled point set for partitioning.
- Cell assignments are read-only on the partition result, so they remain consistent with `Cells`.
- `Cells` is a read-only semantic result in source-site order, with empty cells handled by the
  selected policy. `SiteIndex` matches the cell's index in this result, without gaps.
- On `VoronoiHexPartitionMap`, `ToMutableHexMap()` returns a mutable caller-owned `HexMap<int>`
  containing a copy of the cell IDs. On the masked map, it returns a `HexMap<int?>` with nullable IDs.

```csharp
var voronoiPartition = hexCenters.ToVoronoiHexPartitionMap(sites);
int cellId = voronoiPartition[new VectorXYInt(1, 0)];
VoronoiCell assignedCell = voronoiPartition.Cells[cellId];
HexMap<int> editableIds = voronoiPartition.ToMutableHexMap();
```

```csharp
var maskedPartition = hexCenters.ToVoronoiHexPartitionMap(sites, participationMask);
IPartialHexPartition partial = maskedPartition;
int? cellId = partial[new VectorXYInt(1, 0)];
if (cellId.HasValue)
{
    VoronoiCell assignedCell = maskedPartition.Cells[cellId.Value];
}
HexMap<int?> editableIds = maskedPartition.ToMutableHexMap();
```

## Empty Cells

Pass `EmptyCellPolicy` from `Akeldov.Math.Spatial2D.Partitioning.Voronoi` to
`VoronoiHexPartitioner` or `ToVoronoiHexPartitionMap`:

- `LeaveAsIs` preserves empty cells. Calls without a policy keep this behavior.
- `Exclude` removes empty cells and renumbers the remaining `SiteIndex` and `Id` values from zero.
  Per-hex IDs index the corresponding cells in the compacted `Cells` list.
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

## Combined Participation and Region Masks

```csharp
var selectedRegions = hexCenters.ToVoronoiHexPartitionMap(
    sites, participationMask, regionMask, EmptyCellPolicy.Exclude);
```

Both masks must match the center-map topology. Only participating hexes are assigned, and each
can select only a site in its own region. Excluded hexes have null assignments and do not occur
in any cell's `HexIndexes`. Site eligibility comes from the region mask: a site in an excluded
hex can still receive participating hexes in the same region. Sites outside the map receive no
hexes. Missing eligible sites in a participating region cause `InvalidOperationException`;
empty cells follow the selected policy after assignment.

## Weighted Sites

- Larger weights can pull farther centers into a cell.
- Zero-weight sites only receive exact site points.
- Infinite-weight sites are handled as a special nearest-site case.
