# `HexMap<TValue>`

`HexMap<TValue>` is the general-purpose hex-indexed value storage type.

## Storage

- Stores values for a rectangular hex index domain.
- Uses topology-backed indexing.
- Exposes layout and dimension metadata.

## Access

- Supports coordinate-based value lookup.
- Validates out-of-bounds indexes.
- Reuses shared flat-index mapping.

## Specialization

- `BoolHexMap` adds cell-wise `!`, `&`, `|`, `^`, and conditional `Select` operations.
- Version 0.6.0 adds [binary serialization of Boolean maps](../utility-structures/binary-helpers.md#boolean-maps), preserving topology and values in a versioned format.
- Boolean maps provide hex morphology (`Dilate`, `Erode`, `Open`, `Close`, and `Outline`), dilation by a specified number of rings, plus linear-time flood fill, component labeling, and distance transforms.
- `IntHexMap` and `FloatHexMap` add `Min`, `Max`, and cell-wise arithmetic.
- Version 0.6.0 adds [binary serialization of integer maps](../utility-structures/binary-helpers.md#integer-maps), preserving topology and the full signed `Int32` value range.
- Version 0.6.0 adds [binary serialization of floating-point maps](../utility-structures/binary-helpers.md#floating-point-maps), preserving topology and exact `Single` value bits, including signed zero, infinities, and NaN payloads.
- `MapValues` transforms either each source value or each cell's partial six-neighbor set. Boolean,
  integer, and floating-point selectors return the matching specialized map; other selectors return
  `HexMap<TResult>`. Spatial sources preserve their geometry.
- `Min` and `Max` each scan the map in O(N) time; `GetMinMax` obtains both extrema in one pass, and `TryGetMinMax` handles empty maps without throwing.
- `IntHexMap` and `FloatHexMap` support cell-wise unary negation with `-`.
- Mixed `FloatHexMap` and `IntHexMap` addition and subtraction return a `FloatHexMap` in either operand order; the spatial counterparts return `SpatialFloatHexMap` and require equal geometry.
- `IntHexMap` and `FloatHexMap` support cell-wise multiplication of two maps with `*`.
- Mixed `FloatHexMap` and `IntHexMap` multiplication returns a `FloatHexMap` in either operand order.
- `FloatHexMap` supports cell-wise division of two maps with `/` using floating-point semantics.
- `IntHexMap` supports cell-wise division of two maps with `/` using integer semantics.
- `IntHexMap` supports cell-wise remainder after division by another map or an integer constant with `%`.
- Mixed `FloatHexMap` and `IntHexMap` division returns a `FloatHexMap` in either operand order.
- `IntHexMap` and `FloatHexMap` support cell-wise `<`, `>`, `<=`, and `>=` comparisons with an
  integer or floating-point constant, respectively, in either operand order. The result is a
  `BoolHexMap`; spatial operands return a `SpatialBoolHexMap` and preserve their geometry.
- `Clamp(min, max)` creates a new map with every numeric value restricted to the inclusive range.
- `Rescale(newMin, newMax)` linearly maps the current minimum and maximum to a new inclusive range.
  A constant map is filled with `newMin`; integer results use midpoint-to-even rounding. Spatial
  overloads preserve the source geometry.
- `ToBoolHexMap`, `ToIntHexMap`, and `ToFloatHexMap` create independent mutable copies of interface-typed maps.
- `ToSpatialHexMap(geometry)` copies Boolean, integer, or floating-point maps into the corresponding spatial specialization; the supplied geometry must have the same topology.
- `ToHexMap()` copies a Boolean, integer, or floating-point spatial map back to its corresponding topology-only specialized type.
- `ToSpatialFloatHexMap()` converts spatial integer values to floating point, while
  `ToSpatialIntHexMap()` truncates spatial floating-point values toward zero; both preserve geometry.
- `ToValueMask(values)` converts an integer map into a Boolean mask that selects the listed values.
- `SpatialBoolHexMap`, `SpatialIntHexMap`, and `SpatialFloatHexMap` provide the same operator surface while preserving `HexMapGeometry` in every result.
- Version 0.6.0 adds [binary serialization of spatial maps](../utility-structures/binary-helpers.md#spatial-maps) through `Write` and matching readers, preserving topology, origin, radius, and Boolean, integer, or floating-point values.
- `HexMapFile` provides [file reading and writing](../utility-structures/binary-helpers.md#map-files) for all six map variants, accepting absolute and relative paths.
- Cross-operators combine one spatial specialized map with one topology-only specialized map in either operand order; the result is spatial and retains the spatial operand's geometry.

All specialized maps inherit `HexMap<TValue>`, retain the same topology-backed indexing contract,
and return new maps from their operators without modifying the inputs.

Binary operators between two spatial maps require equal topology, origin, and radius. Cross-operators
between one spatial and one topology-only map require equal topology; the topology-only operand does
not introduce an origin or radius that could conflict with the spatial operand.

```csharp
var topology = new HexMapTopology(4, 3, Layout.OddR);
var land = new BoolHexMap(topology, new bool[topology.Count]);
var landCost = new IntHexMap(topology, new int[topology.Count]);
var waterCost = new IntHexMap(topology, new int[topology.Count]);

IntHexMap movementCost = land.Select(landCost, waterCost);
IntHexMap adjustedCost = (movementCost + 2) * 3;
```

## Map values and neighborhoods

`MapValues` creates independent mutable storage without changing the source. A selector can receive
each value directly or a `PartialSextuplet<TValue>` containing the six edge-adjacent values. At map
boundaries, missing neighbors are marked absent and carry `default(TValue)`.

```csharp
IntHexMap doubledCost = movementCost.MapValues(value => value * 2);
BoolHexMap hasExpensiveNeighbor = movementCost.MapValues(neighbors =>
    (neighbors.HasAdjacent0 && neighbors.Adjacent0 >= 8) ||
    (neighbors.HasAdjacent1 && neighbors.Adjacent1 >= 8));
```

The neighbor positions `Adjacent0` through `Adjacent5` correspond to `HexEdge.Edge0` through
`HexEdge.Edge5` for the topology's layout. Spatial overloads validate the source geometry and retain
it in the result.

## Sample Spatial2D fields

Call `ToSpatialHexMap` on an `IFloatField` or `IIntField` to sample it at every hex center. Pass a
`HexMapGeometry`, or reuse a precomputed `HexCenterMap` when several fields share the same geometry.
The result is a new mutable `SpatialFloatHexMap` or `SpatialIntHexMap`.

`FloatFieldRange` and `IntFieldRange` have matching overloads that draw one value from the sampled
pointwise bounds for every center using a caller-supplied `Random`. Cells are processed in row-major
order. Floating-point values use the interpolation factor returned by `Random.NextDouble`; integer
bounds are inclusive. Invalid or reversed bounds fail at the first affected cell.

## Boolean morphology and connectivity

Boolean morphology uses each cell and its six edge-adjacent neighbors. The finite map is treated as
the complete domain, so missing neighbors beyond its boundary are ignored. `Open` and `Close` execute
two direct passes through an internal pooled scratch buffer; they do not allocate an intermediate map.

```csharp
BoolHexMap expanded = land.Dilate();
BoolHexMap expandedByThreeRings = land.Dilate(ringsCount: 3);
BoolHexMap erodedByThreeRings = land.Erode(ringsCount: 3);
BoolHexMap cleaned = land.Open();
BoolHexMap boundary = land.Outline();

BoolHexMap selectedRegion = land.FloodFill(new VectorXYInt(4, 3));
(IntHexMap labels, int componentCount) = land.ConnectedComponents();
IntHexMap distanceToWater = land.DistanceTransform(targetValue: false);
```

`Dilate(ringsCount)` includes all cells within the given number of hex steps of any `true` cell,
clipped to the existing map. Zero returns an independent copy; negative counts are rejected.
For multiple rings, a bounded breadth-first traversal visits each reached cell at most once,
using a pooled queue and O(N) time and space for N map cells, instead of repeating full-map passes.
The source remains unchanged, and spatial maps retain their geometry.

`land.Dilate(maxDilateDistanceMap)` accepts any `IHexMap<int>` implementation with the same
topology. Each value is the maximum number of steps allowed to reach that cell from the original
`true` area. Every cell along the path must admit its arrival distance: a cell with limit `1`
cannot be entered at step `2`, and expansion cannot pass through it. An alternative route is
allowed, but its full length counts against each cell's limit. Zero and negative limits block
entry into `false` cells; original `true` cells are always retained and act as distance-zero sources.
The method uses O(N) time and space, leaves both inputs unchanged, and preserves the Boolean map's
geometry for spatial results. Limits are matched by cell index.

`Erode()` expands the `false` area by one ring, shrinking the `true` area. `Erode(ringsCount)`
and `Erode(maxErodeDistanceMap)` mirror the dilation overloads, propagating from original `false`
cells instead. Zero rings returns an independent copy; negative ring counts are rejected.
For the limit-map overload, zero and negative limits protect `true` cells from erosion and block
propagation through them. Detours count their full length, and original `false` cells remain false
regardless of their limits. Both overloads use O(N) time and space without repeated whole-map erosion
passes or intermediate inverted maps. They preserve spatial geometry and leave the inputs unchanged.
Missing neighbors outside the map are ignored, so an entirely `true` map remains `true`.

`FloodFill` selects the connected region having the same Boolean value as its seed. Component label
zero represents `false`; `true` components receive deterministic positive labels in row-major discovery
order. `DistanceTransform` contains the minimum number of hex steps to the requested value, or
`int.MaxValue` when that value is absent. Spatial overloads preserve the source geometry and return the
corresponding spatial specialization.

## Perlin noise generation

`CreatePerlinNoise` samples deterministic fractal Perlin noise at the physical center of each
unit-radius hex and returns a mutable `FloatHexMap` with values in the `[0, 1]` range. Sampling
hex centers keeps the field spatially coherent across offset rows and columns in every layout.

```csharp
var topology = new HexMapTopology(128, 96, Layout.OddR);

FloatHexMap heights = topology.CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5,
    persistence: 0.5f,
    lacunarity: 2f);
```

Larger `scale` values produce broader features. Use `offset` to sample another part of the same
deterministic noise field, for example when generating adjacent map chunks.

Apply `GaussianBlur` when the generated or supplied floating-point field needs smoothing:

```csharp
FloatHexMap smoothHeights = heights.GaussianBlur(sigma: 1.25f);
```

The operation returns a new map, normalizes its kernel at map boundaries, and leaves the source
map unchanged. An overload with `radius` allows explicit kernel truncation in hex steps.
