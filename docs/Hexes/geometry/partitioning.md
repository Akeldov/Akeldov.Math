# Partitioning

Partitions group hex indexes. Voronoi partitioning assigns hex centers to weighted sites.

## Partition Cells and Partitions

`IHexPartitionCell` in `Akeldov.Math.Hexes` represents one part of a partition and exposes the
read-only `HexIndexes` sequence. `HexPartitionCell` implements this contract by copying the
constructor input into a read-only snapshot.
Later changes to the input collection do not affect the cell, and the returned collection
cannot be modified through a mutable collection interface. Empty cells are allowed;
input order and duplicate indexes are preserved. Index bounds and map membership are not validated.

`VoronoiCell` derives from `HexPartitionCell`, so code that needs only the assigned hex indexes
can accept `IHexPartitionCell` for both manually constructed cells and Voronoi cells.

`IHexPartition` represents the whole partition through its read-only `Cells` collection.
`HexPartition` copies the supplied collection, retaining the cell objects in their original
order. It accepts an empty collection and empty cells, but rejects null collections and null
cells. It does not validate map coverage or overlap between cells. Custom `IHexPartitionCell`
implementations are retained as-is, so their own contract determines whether their state can change.

```csharp
IHexPartitionCell cell = new HexPartitionCell(
    new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) });
IHexPartition partition = new HexPartition(new[] { cell });
IReadOnlyList<VectorXYInt> indexes = partition.Cells[0].HexIndexes;
```

## Voronoi Cells

- `VoronoiCell` stores the site index and assigned hex indexes.
- Empty cells are preserved by default when a site receives no hexes.
- Cell inputs are validated before construction.

## Partition Maps

- `VoronoiHexPartitionMap` stores Voronoi cells in a hex map.
- `MaskedVoronoiHexPartitionMap` stores nullable assignments for a masked partition.
- Both maps implement `IHexPartition`, exposing the same cell objects through the common
  contract while preserving their strongly typed `IReadOnlyList<VoronoiCell>` properties.
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
