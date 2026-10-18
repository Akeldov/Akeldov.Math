# Partition and Diffuse Territories

Use `PartitionByPerlinNoise` to assign territories by relative area, then
`DiffuseByCellularPotts` to evolve their boundaries. Both operations return new maps.

## Create Territories from Noise

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(64, 48, VectorXY.Zero, 1f, Layout.OddR);
var targets = new[]
{
    new PerlinNoisePartitionTarget(7, 30),
    new PerlinNoisePartitionTarget(42, 50),
    new PerlinNoisePartitionTarget(99, 20),
};
var noise = new PerlinNoisePartitionParameters(scale: 18f);
var full = geometry.PartitionByPerlinNoise(targets, noise, seed: 42);
```

Target order selects bands from low to high noise. Shares are normalized; 30/50/20 and
0.3/0.5/0.2 are equivalent. Equal noise values remain together, so actual counts can differ
from the requested proportions. IDs need not be consecutive: find a cell by its `Id`, not
by indexing `Cells` with an arbitrary territory ID. Noise scale uses world-space units.

## Exclude Hexes and Partition Each Region

```csharp
var participation = new BoolHexMap(geometry.Topology);
var regions = new IntHexMap(geometry.Topology);
for (int y = 0; y < 48; y++)
for (int x = 0; x < 64; x++)
{
    var index = new VectorXYInt(x, y);
    participation[index] = x > 0 && y > 0;
    regions[index] = x < 32 ? 0 : 1;
}

var partial = geometry.PartitionByPerlinNoise(participation, regions, targets, noise, seed: 42);
// Without regions: geometry.PartitionByPerlinNoise(participation, targets, noise, seed: 42).
```

Thresholds and proportions are calculated separately in each region. A target's cell collects
its hexes from all regions. You can instead pass a nullable region map: null excludes a hex,
and every integer labels a region. Masks must have the same topology as the geometry.

## Diffuse Boundaries Without Crossing Regions

```csharp
var diffusion = new CellularPottsParameters(
    stepCount: 20, temperature: 0.5f, boundaryEnergy: 1f, areaPenalty: 1f,
    connectivity: CellularPottsConnectivity.None);

var evolved = partial.DiffuseByCellularPotts(regions, diffusion, new Random(42));
var evolvedFull = full.DiffuseByCellularPotts(regions, diffusion, new Random(42));

var targetAreas = new Dictionary<int, float> { [7] = 800f };
var resized = partial.DiffuseByCellularPotts(regions, targetAreas, diffusion, new Random(42));
```

Noise territories may start disconnected, so this example explicitly uses `None`. The default
`Preserve` policy requires every non-empty cell to be six-connected initially and prevents it
from fragmenting or disappearing. Empty cells cannot acquire hexes.

Each step makes one copy attempt per participating hex, sampled with replacement. Region IDs
restrict copies between neighbors; they do not isolate energy, area, or connectivity calculations.
Null hexes stay null and contribute no boundary energy. There is no additional participation mask:
the source assignments determine participation. Source assignments must agree with `Cells`.

Area targets are soft preferences in hex counts. Omitted IDs use their initial areas. Neither
exact target counts nor a minimum-energy result are guaranteed. Without region restrictions,
omit `regions` in either the full or partial overload. Supply a seeded `Random` for repeatability.

## Split Disconnected Cells

```csharp
var connected = evolved.SplitDisconnectedCells();
var connectedFull = evolvedFull.SplitDisconnectedCells();
var preserved = connected.DiffuseByCellularPotts(
    new CellularPottsParameters(stepCount: 10), new Random(42));
```

Splitting creates one `HexPartitionCell` per six-connected component, assigns IDs from zero
in row-major discovery order, omits empty cells, and preserves nulls. It does not retain source
territory IDs or specialized metadata. Split before diffusion when each component should become
an independently preserved cell. Diffusion itself preserves source cell IDs and order and returns
new cell snapshots; it does not modify the source map.
