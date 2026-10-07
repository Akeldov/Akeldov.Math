# Partition a Map by Nearest Sites

Use `ToVoronoiHexPartitionMap()` to assign every hex to the nearest weighted site. The comparison
uses the world-space position of each hex center, so create a
<xref:Akeldov.Math.Hexes.Geometry.HexCenterMap> from the same geometry that places your map.

## Create the partition

The following example divides a one-row map between two equally weighted sites:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

var geometry = new HexMapGeometry(
    width: 3,
    height: 1,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var centers = new HexCenterMap(geometry);
var sites = new[]
{
    new Site(new PointXY(0f, 0f), weight: 1f),
    new Site(new PointXY(4f, 0f), weight: 1f)
};

VoronoiHexPartitionMap partition =
    centers.ToVoronoiHexPartitionMap(sites);
```

Site positions and hex centers must use the same coordinate space. A site does not have to be
inside the map: it can still receive hexes whose centers are closest to it.

## Exclude Hexes with a Participation Mask

Pass a Boolean map with the same topology to partition only selected centers:

```csharp
var participationMask = new BoolHexMap(
    geometry.Topology,
    new[] { true, false, true });

MaskedVoronoiHexPartitionMap maskedPartition =
    centers.ToVoronoiHexPartitionMap(sites, participationMask);
```

Participating hexes receive their nearest weighted cell. Excluded hexes return `null`, report
`false` from `Participates`, and do not appear in any cell's `HexIndexes`. The result copies the
mask; later source-map changes do not affect it. Use `ToMutableParticipationMask()` or
`ToMutableHexMap()` when independent mutable copies are needed.

## Read assignments and cells

Index the partition map to find the cell assigned to a particular hex. `SiteIndex` refers to the
site's position in the original `sites` array:

```csharp
for (int x = 0; x < geometry.Topology.Resolution.X; x++)
{
    VoronoiCell cell = partition[new VectorXYInt(x, 0)];
    Console.Write($"{cell.SiteIndex} ");
}
```

The example prints:

```text
0 0 1
```

Use `Cells` when you need the hexes grouped by source site:

```csharp
foreach (VoronoiCell cell in partition.Cells)
{
    Console.WriteLine($"Site {cell.SiteIndex}: {cell.HexIndexes.Count} hexes");
}
```

The result is:

```text
Site 0: 2 hexes
Site 1: 1 hexes
```

`Cells` contains one cell per source site in source order. A site that receives no hexes is still
present with an empty `HexIndexes` list.

## Reuse a site set

The extension method creates a partitioner for one call. Construct
<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitioner> directly when the same sites
will be applied to several map geometries:

```csharp
var partitioner = new VoronoiHexPartitioner(sites);

VoronoiHexPartitionMap firstPartition = partitioner.Partition(centers);
VoronoiHexPartitionMap secondPartition = partitioner.Partition(
    new HexCenterMap(new HexMapGeometry(6, 4, VectorXY.Zero, 1f, Layout.OddR)));
```

The constructor copies and validates the sites, so later changes to the source array do not alter
the partitioner.

## Adjust influence with weights

For positive finite weights, assignment compares `distance / weight`. Increasing a site's weight
therefore expands its influence. Positions must be finite, weights must be non-negative and not
`NaN`, the site list must not be empty, and at least one weight must be nonzero.

<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitionMap> is a read-only semantic
result: its per-hex assignments remain consistent with `Cells` and each cell's `HexIndexes`. Call
`ToMutableHexMap()` when you need a new caller-owned map whose assignments can be changed.

For the weighted-distance formula and the special behavior of zero and infinite weights, see
[Space Partitioning](../../concepts/spatial-algorithms/space-partitioning.md).
