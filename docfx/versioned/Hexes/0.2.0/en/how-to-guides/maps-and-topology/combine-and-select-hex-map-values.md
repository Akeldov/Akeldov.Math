# Combine and Select Hex-Map Values

Use `BoolHexMap` operators to combine masks, then use `Select` to choose one of two values for
each cell.

## Combine masks

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
```

The operators work cell by cell and create new maps. Both operands must have equal topologies.

## Select numeric values

```csharp
var landCost = new IntHexMap(topology, new[] { 1, 1, 1, 2, 2, 2 });
var waterCost = new IntHexMap(topology, new[] { 8, 8, 8, 9, 9, 9 });

IntHexMap movementCost = land.Select(landCost, waterCost);
```

Where `land[index]` is `true`, the result receives `landCost[index]`; otherwise it receives
`waterCost[index]`. Overloads return `BoolHexMap`, `IntHexMap`, or `FloatHexMap` for specialized
branches.

The generic overload accepts two `HexMap<TValue>` instances:

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

`Select` requires all three topologies to match and returns a new non-spatial map. Source maps are
not modified.
