# Combine and Select Hex-Map Values

Use Boolean-map operators to combine masks, then use `Select` to choose one of two values for each
cell. Topology-only overloads return topology-only maps; specialized spatial overloads preserve
geometry.

## Combine Masks

```csharp
using Akeldov.Math.Hexes;

var topology = new HexMapTopology(3, 2, Layout.OddR);

var land = new BoolHexMap(topology, new[]
{
    true,  true,  false,
    true,  false, false,
});

var visible = new BoolHexMap(topology, new[]
{
    true, false, true,
    true, true,  false,
});

BoolHexMap visibleLand = land & visible;
BoolHexMap eitherCondition = land | visible;
BoolHexMap exactlyOneCondition = land ^ visible;
BoolHexMap hidden = !visible;
```

The operators work cell by cell and create new maps. Topology-only operands must have equal
topologies.

## Select Topology-Only Values

```csharp
var landCost = new IntHexMap(topology, new[] { 1, 1, 1, 2, 2, 2 });
var waterCost = new IntHexMap(topology, new[] { 8, 8, 8, 9, 9, 9 });

IntHexMap movementCost = land.Select(landCost, waterCost);
```

Where `land[index]` is `true`, the result receives `landCost[index]`; otherwise it receives
`waterCost[index]`. Specialized overloads return `BoolHexMap`, `IntHexMap`, or `FloatHexMap`.

The generic topology-only overload accepts two `HexMap<TValue>` instances:

```csharp
var landLabels = new HexMap<string>(topology, new[]
{
    "plain", "forest", "plain", "hill", "hill", "plain",
});
var waterLabels = new HexMap<string>(topology, new[]
{
    "sea", "sea", "lake", "sea", "lake", "sea",
});

HexMap<string> terrain = land.Select(landLabels, waterLabels);
```

All three topologies must match. These overloads return a new topology-only map.

## Select Spatial Values

Use a `SpatialBoolHexMap` condition with matching specialized spatial branches when the result must
retain world-space placement:

```csharp
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 8f);

SpatialBoolHexMap spatialLand = land.ToSpatialHexMap(geometry);
SpatialIntHexMap spatialLandCost = landCost.ToSpatialHexMap(geometry);
SpatialIntHexMap spatialWaterCost = waterCost.ToSpatialHexMap(geometry);

SpatialIntHexMap spatialMovementCost = spatialLand.Select(
    spatialLandCost,
    spatialWaterCost);
```

Spatial specialized overloads are available for Boolean, integer, and floating-point branches.
The condition, `whenTrue`, and `whenFalse` maps must have equal geometry, including topology,
origin, and radius. The result is spatial and preserves that geometry. There is no generic
`SpatialHexMap<TValue>` selection overload.

Every `Select` overload creates independent result storage and does not modify its sources.
