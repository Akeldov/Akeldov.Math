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

## Creating a Partition

The following example divides a three-hex map between two equally weighted sites:

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

VoronoiCell assignedCell = partition[new VectorXYInt(1, 0)];
int sourceSiteIndex = assignedCell.SiteIndex;
```

`ToVoronoiHexPartitionMap` is shorthand for constructing
<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitioner> and calling `Partition`.
Create a `VoronoiHexPartitioner` directly when one site set will be reused with several center
maps. Its constructor copies and validates the site list.

## Partition Map

<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitionMap> is a consistent read-only
result that implements `ISpatialHexMap<VoronoiCell>`.

| Member | Contents |
|---|---|
| `Centers` | The source center map used by the partition |
| `Topology` and `Geometry` | The topology and geometry of that center map |
| `partition[index]` | The Voronoi cell assigned to the specified hex |
| `Cells` | One Voronoi cell for each source site, in source order |

Every hex assigned to the same site refers to one
<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiCell> object. Its data has the following
meaning:

- `SiteIndex` is the index in the source site list;
- `Site` is the copied site value;
- `Center` is the same position as `Site.Position`;
- `HexIndexes` is the read-only list of assigned indices.

`Cells.Count` always equals the number of sites. If a site receives no centers, its cell is
preserved with an empty `HexIndexes` list. This keeps cells aligned with source sites without an
additional lookup.

## Mutable Copy

Assignments in `VoronoiHexPartitionMap` cannot diverge from the grouping in `Cells` because the
result exposes no setters. If application-specific reclassification is needed after partitioning,
call `ToMutableHexMap()`. It returns a new caller-owned `HexMap<VoronoiCell>` containing a copy of
the assignments.

Changing that map does not modify the original result or its `HexIndexes` lists. If the
application needs rebuilt groups, construct a new model from the changed assignments instead of
treating the old `Cells` as current for the copy.

Return to the [Spatial Algorithms overview](index.md), or continue to
[Chromatization](chromatization.md) when classification must depend only on logical index and
layout.
