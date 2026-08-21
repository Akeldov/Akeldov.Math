# Maps

A hex map associates one value with every cell of a rectangular
<xref:Akeldov.Math.Hexes.HexMapTopology>. The topology fixes the valid X/Y indices, layout, and
cell count; the map supplies the data. Use maps for terrain, masks, costs, labels, or any other
per-cell state.

## Choose a map type

| Type | Purpose |
|---|---|
| `IHexMap<TValue>` | Read-only access to any topology-backed map |
| `HexMap<TValue>` | Mutable storage for arbitrary values |
| `ISpatialHexMap<TValue>` | Read-only access to values with world-space geometry |
| `SpatialHexMap<TValue>` | Mutable values with retained world-space geometry |
| `BoolHexMap` | Mutable Boolean mask with `&`, `\|`, `^`, and `Select` |
| `IntHexMap` | Mutable integer data with `Min`, `Max`, and arithmetic operators |
| `FloatHexMap` | Mutable floating-point data with numeric, noise, and blur operations |

The specialized maps introduced in Hexes 0.2.0 inherit `HexMap<TValue>`. They therefore keep the
same indexing and storage rules and can be passed to APIs that accept `IHexMap<bool>`,
`IHexMap<int>`, or `IHexMap<float>`.

## Create and index a map

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var terrain = new HexMap<string?>(topology);
var cell = new VectorXYInt(2, 1);

terrain[cell] = "forest";
string? value = terrain[cell];
```

The topology-only constructor allocates `Topology.Count` cells initialized with
`default(TValue)`. Two index forms address the same row-major storage:

- `map[new VectorXYInt(x, y)]` checks the X/Y coordinates;
- `map[flatIndex]` uses `flatIndex = y * width + x`.

An invalid index throws `IndexOutOfRangeException`. The layout changes how X/Y indices map to the
hex grid, but does not change storage order.

## Initialize from an array

```csharp
var elevation = new IntHexMap(topology, new[]
{
    10, 11, 12, 13,
    20, 21, 22, 23,
    30, 31, 32, 33,
});
```

The array length must equal `topology.Count`. Array constructors retain the supplied array without
copying it, so the caller and map share mutable storage. Pass a cloned array when the map needs
independent storage. `ToIntHexMap()` and `ToFloatHexMap()` instead create independent copies from
any compatible `IHexMap<TValue>`.

## Work with numeric maps

`IntHexMap` and `FloatHexMap` expose `Min` and `Max`. Reading either property from an empty map
throws `InvalidOperationException`.

Their operators evaluate every cell and return a new map without changing the inputs:

```csharp
var baseCost = new IntHexMap(topology, new int[topology.Count]);
IntHexMap adjustedCost = (baseCost + 3) * 2;

var height = new FloatHexMap(topology, new float[topology.Count]);
FloatHexMap normalized = (height - height.Min) / (height.Max - height.Min);
```

Both numeric types support map-to-map `+` and `-`, scalar `+`, `-`, `*`, and `/`, plus scalar-first
addition, subtraction, and multiplication. Map operands must have equal topologies. Integer
operations are checked for overflow, and integer division follows normal C# division rules.

The normalization example assumes `height.Max != height.Min`; validate that condition before
dividing real data.

## Combine masks and select values

`BoolHexMap` provides cell-wise AND, OR, and XOR operators. Use `Select` to choose between two maps
cell by cell:

```csharp
var land = new BoolHexMap(topology, new bool[topology.Count]);
var visible = new BoolHexMap(topology, new bool[topology.Count]);
BoolHexMap visibleLand = land & visible;

var landCost = new IntHexMap(topology, new int[topology.Count]);
var waterCost = new IntHexMap(topology, new int[topology.Count]);
IntHexMap movementCost = land.Select(landCost, waterCost);
```

`Select` supports Boolean, integer, floating-point, and generic `HexMap<TValue>` branches. All
three maps must have equal topologies. It returns a new non-spatial map and does not modify its
inputs.

The existing `And` and `Or` extensions remain available for interface-typed and spatial Boolean
maps. Their spatial overloads preserve geometry; operators on `BoolHexMap` produce non-spatial
maps.

## Generate and smooth floating-point fields

Generate deterministic fractal Perlin noise directly from a topology:

```csharp
FloatHexMap noise = topology.CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5,
    persistence: 0.5f,
    lacunarity: 2f);

FloatHexMap smoothNoise = noise.GaussianBlur(sigma: 1.25f);
```

`CreatePerlinNoise` samples unit-radius hex centers and returns values in `[0, 1]`. Larger `scale`
values produce broader features; `offset` selects another portion of the same deterministic field.

`GaussianBlur` returns a new map and normalizes its kernel at map boundaries. The default overload
truncates the kernel at three standard deviations; the overload with `radius` lets callers choose
the non-negative radius in hex steps.

## Retain world-space geometry

Use `SpatialHexMap<TValue>` when values must carry a
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>. Its `Topology` comes from the geometry, while
`Geometry` also preserves origin and hex radius:

```csharp
using Akeldov.Math.Hexes.Geometry;

var geometry = new HexMapGeometry(topology, new VectorXY(100f, 50f), radius: 8f);
var moisture = new SpatialHexMap<float>(geometry);
moisture[new VectorXYInt(0, 0)] = 0.75f;
```

Specialized maps are topology-backed rather than spatial. If a transformation must preserve
world-space placement, construct a `SpatialHexMap<TValue>` with the source geometry explicitly.

## Continue with related storage

- [Rasters](rasters.md) describes regular and partial neighborhood storage.
- [Complete and Partial Neighborhoods](complete-and-partial-neighborhoods.md) explains
  boundary-aware adjacency containers.
- [Rasterization](../rasterization.md) converts map values into regular pixel rasters.
- [Spatial Algorithms](../spatial-algorithms/index.md) covers pathfinding, chromatization, and
  partitioning.
