# Partition a Map by Nearest Sites

Create a `HexCenterMap` from the geometry that places your map, then assign centers to weighted sites:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

var geometry = new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR);
var centers = new HexCenterMap(geometry);
var sites = new[]
{
    new Site(new PointXY(0f, 0f), weight: 1f),
    new Site(new PointXY(4f, 0f), weight: 1f),
};

VoronoiHexPartitionMap partition = centers.PartitionByVoronoi(sites);
for (int x = 0; x < geometry.Topology.Resolution.X; x++)
    Console.Write($"{partition[new VectorXYInt(x, 0)]} ");
// 0 0 1

foreach (VoronoiHexPartitionCell cell in partition.Cells)
    Console.WriteLine($"Cell {cell.Id}: {cell.HexIndexes.Count} hexes");
```

Site positions and stored centers must use the same coordinate space. Without a region mask,
a site outside the map can still receive hexes. For finite positive weights, assignment minimizes
`distance / weight`. See [Space Partitioning](../../concepts/spatial-algorithms/space-partitioning.md)
for zero and infinite weights.

## Exclude Hexes with a Participation Mask

```csharp
var participationMask = new BoolHexMap(geometry.Topology, new[] { true, false, true });
PartialVoronoiHexPartitionMap partial =
    centers.PartitionByVoronoi(participationMask, sites);

int? cellId = partial[new VectorXYInt(1, 0)]; // null
bool participated = partial.Participates(new VectorXYInt(1, 0)); // false
```

Excluded hexes initially have null identifiers and are absent from each cell's `HexIndexes`.
The result copies the participation mask; changing the source mask does not change it.
`Participates` always describes the original mask, even after assignment edits.

## Restrict Assignments to Regions

An `IHexMap<int>` region mask limits each hex to sites in the same region. Equal values identify
a region even across disconnected areas; zero and negative values are valid. A site's region is
the hex containing its position. Sites outside the map receive no hexes. A participating hex
without an eligible site in its region throws `InvalidOperationException`.

```csharp
var regions = new IntHexMap(geometry.Topology, new[] { 0, 0, 1 });
VoronoiHexPartitionMap regional =
    centers.PartitionByVoronoi(regions, sites,
        new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.Exclude));
PartialVoronoiHexPartitionMap selected = centers.PartitionByVoronoi(
    participationMask, regions, sites,
    new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.Exclude));

var nullableRegions = new HexMap<int?>(geometry.Topology, new int?[] { 0, null, 1 });
PartialVoronoiHexPartitionMap combined = centers.PartitionByVoronoi(
    nullableRegions, sites,
    new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.Exclude));
```

Both masks must match the center-map topology. With a separate Boolean mask, a site in an excluded
hex can still receive participating hexes in its integer region. In a nullable region mask, null
excludes both the hex and a site located in that hex.

## Reassign Exclaves

```csharp
PartialVoronoiHexPartitionMap connected = centers.PartitionByVoronoi(
    participationMask, sites,
    new VoronoiHexPartitionParameters(
        emptyCellPolicy: EmptyCellPolicy.Exclude,
        exclavePolicy: ExclavePolicy.ReassignToClosestCell));
```

The default `ExclavePolicy.LeaveAsIs` preserves disconnected assignments. Reassignment keeps the
component containing the closest hex to each site, then grows from retained components in layers.
An affected hex chooses an adjacent grown cell by unweighted distance to its site. Ties favor
the earlier source site; region boundaries and excluded hexes remain intact. Unreachable
components become new cells after the source cells, in row-major component order, with a site
at their hex closest to the original site and the original weight.

Empty-cell handling follows reassignment. `LeaveAsIs` retains empty cells, `Exclude` removes them
and compacts identifiers, and `ThrowException` rejects any empty cell. Initial `Id` and `SiteIndex`
index `Cells`; after exclusion or creation of exclave cells they need not index the original sites.

## Edit Assignments or Make a Copy

Full partitions inherit `IntHexMap`; partial partitions inherit `HexMap<int?>`. Indexer writes
change only assignments. The retained `Cells`, their `HexIndexes`, and participation mask continue
to describe the initial result.

```csharp
SpatialHexMap<int?> shared = partial; // O(1), shares geometry and assignments.
HexMap<int?> independent = partial.ToMutableHexMap(); // Copies assignments.
BoolHexMap originalMask = partial.ToMutableParticipationMask(); // Copies the original mask.
```

## Relax Site Positions

Pass `new VoronoiHexPartitionParameters(relaxationIterations: 3)` to enable centroid relaxation.
The default is zero iterations. The same immutable parameter object configures relaxation,
empty-cell handling, and exclave handling for full and partial overloads.

## Migrate from 0.8.0

- Rename `ToVoronoiHexPartitionMap` and `ToPartialVoronoiHexPartitionMap` to `PartitionByVoronoi`.
- Pass participation and region masks before `sites`.
- Replace separate policy arguments with `VoronoiHexPartitionParameters` after `sites`.
- Rename `PairPresenceFlags`, `TripletPresenceFlags`, `SeptupletPresenceFlags`, and `ChromaticTripletPresenceFlags`
  to `PairPresence`, `TripletPresence`, `SeptupletPresence`, and `ChromaticTripletPresence` in raster code.
- Rebuild consumers; these source and binary API changes are incompatible with 0.8.0.

For noise-based territories and subsequent diffusion, see
[Partition and Diffuse Territories](partition-and-diffuse-territories.md).
