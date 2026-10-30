# Migrate from Hexes 0.9.0

Hexes 0.10.0 unifies spatial partition results. Rebuild consumers: the result-type changes
are source and binary incompatible with 0.9.0.

## Replace explicit result types

| 0.9.0 | 0.10.0 |
|---|---|
| `VoronoiHexPartitionMap` | `SpatialHexPartitionMap<VoronoiHexPartitionCell>` |
| `PerlinNoiseHexPartitionMap` | `SpatialHexPartitionMap<HexPartitionCell>` |
| `PartialPerlinNoiseHexPartitionMap` | `SpatialPartialHexPartitionMap<HexPartitionCell>` |

The replacement types live in `Akeldov.Math.Hexes`. `VoronoiHexPartitionCell` remains in
`Akeldov.Math.Hexes.Partitioning.Voronoi`. Calls using `var` generally need no result-type edit.
`PartialVoronoiHexPartitionMap` remains available and now derives from the spatial partial base.

## Replace full Voronoi convenience members

Keep the original `HexCenterMap` variable if you need the exact stored centers used for the
partition. The full result no longer exposes `Centers`. Its `Geometry` remains available;
creating a new center map from geometry does not reproduce manually edited center values.

Use the full partition directly where `ISpatialIntHexMap` is accepted. The former implicit
conversion to `SpatialHexMap<int>` and `ToMutableHexMap()` are removed. To obtain an independent
spatial assignment map, copy the identifiers explicitly:

```csharp
var assignments = new int[partition.Topology.Count];
for (int i = 0; i < assignments.Length; i++)
    assignments[i] = partition[i];
var independent = new SpatialHexMap<int>(partition.Geometry, assignments);
```

The partial Voronoi result retains its center map, shared-facade conversion, assignment-copy
method, and original participation-mask accessors.

## Retain geometry through transformations

`SplitDisconnectedCells` and `DiffuseByCellularPotts` have overloads for both spatial partition
types. Keep the spatial static type, or use `var`, to retain origin, radius, and layout in the
result. Upcasting to `HexPartitionMap<T>` or `PartialHexPartitionMap<T>` selects the topology-only
extensions. Neither operation mutates the source partition.

Partition indexer writes still update only assignments, not retained `Cells` or their
`HexIndexes`. Keep these consistent before diffusion or splitting.

## Other additions

- `IHexMap<out TValue>` supports covariant conversions for reference-type values.
- `ToOffsetRegion(distance, layout)` expands a polyhex region by a finite, non-negative distance
  in world-space units. Zero produces the original shape; `ToApothemOffsetRegion` remains available.
- Target frameworks remain `netstandard2.1` and `net6.0`; the Spatial2D dependency remains
  `[1.1.0, 2.0.0)`.
