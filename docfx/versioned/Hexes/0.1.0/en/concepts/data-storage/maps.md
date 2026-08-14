# Maps

A hex map associates one value with every cell of a rectangular
<xref:Akeldov.Math.Hexes.HexMapTopology>. The topology fixes the valid row-and-column indices,
their layout, and the number of stored values; the map supplies the data. Use maps for terrain,
masks, costs, labels, or any other per-cell state.

## Choose a map type

Hexes 0.1.0 provides two general-purpose map implementations and their read-only interfaces:

| Type | Stored metadata | Value access | Typical role |
|---|---|---|---|
| `IHexMap<TValue>` | Topology | Read-only | Accept any topology-backed map as input |
| `HexMap<TValue>` | Topology | Read and write | Store arbitrary values per cell |
| `ISpatialHexMap<TValue>` | Topology and geometry | Read-only | Accept a map placed in world space |
| `SpatialHexMap<TValue>` | Topology and geometry | Read and write | Store values that must retain their physical placement |

The interfaces expose get-only indexers. They restrict how a consumer can access a map, but do
not make a mutable implementation or its stored reference-type values immutable.

In this package version, Boolean, integer, and floating-point maps are generic instantiations:
`HexMap<bool>`, `HexMap<int>`, and `HexMap<float>`. There are no separate `BoolHexMap`,
`IntHexMap`, or `FloatHexMap` types in Hexes 0.1.0.

## Create and update a map

Construct a map from the topology shared by its cells. The topology-only constructor allocates a
new backing array and initializes every entry with `default(TValue)`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var terrain = new HexMap<string?>(topology);
var cell = new VectorXYInt(2, 1);

terrain[cell] = "forest";
string? value = terrain[cell];

IHexMap<string?> readOnlyTerrain = terrain;
string? sameValue = readOnlyTerrain[cell];
```

`Topology` is retained as part of the map. Because `HexMapTopology` is an immutable value type,
several maps can safely carry equal topology values and address the same logical cells.

## Use coordinate and flat indices

Both map interfaces support two index forms:

- `map[new VectorXYInt(x, y)]` addresses a row-and-column index and checks both components against
  the topology resolution;
- `map[flatIndex]` addresses the backing sequence directly in row-major order.

For a topology of width `W`, the correspondence is:

```text
flatIndex = y * W + x
x = flatIndex % W
y = flatIndex / W
```

`X` advances first, and every valid flat index lies from `0` through `Topology.Count - 1`.
The map layout does not change this storage order; it changes how `(x, y)` is interpreted on the
hex grid.

```csharp
var index = new VectorXYInt(2, 1);
int flatIndex = index.Y * topology.Resolution.X + index.X; // 6

terrain[index] = "water";
bool sameCell = terrain[flatIndex] == "water";             // true
```

An invalid `VectorXYInt` or flat index throws `IndexOutOfRangeException`. Coordinate conversion
and neighborhood helpers can produce cells outside a finite topology, so check bounds before
indexing when a value was not derived from a known map cell. See
[Topology](../hex-grid-model/topology.md) for the exact rectangular domain.

## Initialize from existing values

Pass an array when values already exist in row-major order. Its length must equal
`topology.Count`:

```csharp
var topology = new HexMapTopology(3, 2, Layout.EvenR);
var values = new[]
{
    10, 11, 12, // y = 0
    20, 21, 22, // y = 1
};

var elevation = new HexMap<int>(topology, values);

elevation[new VectorXYInt(1, 1)] = 99;
bool sharedStorage = values[4] == 99; // true
```

The array constructor retains the supplied array without copying it. The caller and map therefore
share mutable storage: changes through either reference are visible through the other. Clone the
array before construction when the map must not share its backing array with the caller:

```csharp
var ownedElevation = new HexMap<int>(topology, (int[])values.Clone());
```

The clone is shallow: reference-type `TValue` objects remain shared even though the backing array
does not.

The map does not expose its backing array. A `null` array throws `ArgumentNullException`, and a
length different from `Topology.Count` throws `ArgumentException`.

An empty topology is valid, so a map can contain zero cells. Its backing array is empty and every
index is invalid. The map validates storage shape, not the semantic validity of individual
`TValue` instances.

## Store common value types

`TValue` does not change the storage contract. Choose it to express what one cell means:

```csharp
var blocked = new HexMap<bool>(topology);       // all false
var movementCost = new HexMap<int>(topology);  // all 0
var temperature = new HexMap<float>(topology); // all 0f
var labels = new HexMap<string?>(topology);     // all null
```

In Hexes 0.1.0, `HexMap<int>` and `HexMap<float>` do not add arithmetic, minimum/maximum, filtering,
or noise-generation members. Iterate over the `Topology.Count` flat indices when computing or
transforming their values, or pass them to an algorithm that accepts `IHexMap<TValue>`.

## Retain world-space geometry

Use `SpatialHexMap<TValue>` when the same values must also carry a
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>. Its inherited `Topology` is the geometry's
topology, while `Geometry` also preserves the zero-hex origin and hex radius:

```csharp
using Akeldov.Math.Hexes.Geometry;

var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 8f);

var moisture = new SpatialHexMap<float>(geometry);
moisture[new VectorXYInt(0, 0)] = 0.75f;

ISpatialHexMap<float> spatialInput = moisture;
bool sameTopology = spatialInput.Topology == geometry.Topology; // true
```

The constructor without values allocates a new default-filled array. The array overload has the
same shared-storage contract as `HexMap<TValue>`. `SpatialHexMap<TValue>` rejects geometry whose
origin has a non-finite component or whose radius is not finite and positive. See
[Geometry](../hex-grid-model/geometry.md) for coordinate-space conventions.

## Combine Boolean maps

<xref:Akeldov.Math.Hexes.BooleanHexMapExtensions> provides cell-wise `And` and `Or` operations for
`IHexMap<bool>` and `ISpatialHexMap<bool>`:

```csharp
IHexMap<bool> walkable = new HexMap<bool>(topology, new[]
{
    true,  true,  false,
    false, true,  true,
});

IHexMap<bool> visible = new HexMap<bool>(topology, new[]
{
    true, false, true,
    true, true,  false,
});

HexMap<bool> visibleAndWalkable = walkable.And(visible);
HexMap<bool> visibleOrWalkable = walkable.Or(visible);
```

Both operations allocate a new mutable map owned by the caller and leave their inputs unchanged.
The result retains the common topology. Inputs with different topologies are rejected with
`ArgumentException`, and `null` inputs with `ArgumentNullException`.

Spatial overloads return `SpatialHexMap<bool>` and retain the geometry of the spatial operand.
Two spatial inputs must have equal complete geometries, not merely equal topologies. A mixed
spatial/non-spatial operation requires equal topologies; if the nominally non-spatial operand is
actually spatial at runtime, its geometry must also match.

Overload selection follows the variables' compile-time types. When the result must retain
geometry, at least one operand needs a spatial compile-time type such as `ISpatialHexMap<bool>` or
`SpatialHexMap<bool>`. Hexes 0.1.0 does not provide built-in Boolean negation or exclusive-or
operations.

## Convert values explicitly

There are no implicit conversions between maps with different `TValue` types. Allocate result
values, transform each flat cell, and construct a map with the metadata that must be preserved:

```csharp
var elevation = new HexMap<int>(topology, values);
var normalizedValues = new float[elevation.Topology.Count];

for (int i = 0; i < normalizedValues.Length; i++)
    normalizedValues[i] = elevation[i] / 100f;

var normalized = new HexMap<float>(elevation.Topology, normalizedValues);
```

For a spatial source, construct `SpatialHexMap<TOutput>` with the source `Geometry` instead of
constructing `HexMap<TOutput>` with only its topology. This prevents accidental loss of origin and
scale while changing the cell value type.

## Continue with related storage

- [Rasters](rasters.md) describes regular and partial neighborhood storage.
- [Complete and Partial Neighborhoods](complete-and-partial-neighborhoods.md) explains boundary-aware
  adjacency containers.
- [Rasterization](../rasterization.md) converts map values into regular pixel rasters.
- [Spatial Algorithms](../spatial-algorithms/index.md) covers consumers such as pathfinding,
  chromatization, and space partitioning.
