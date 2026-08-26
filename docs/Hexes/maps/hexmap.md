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
- `IntHexMap` and `FloatHexMap` add `Min`, `Max`, and cell-wise arithmetic.
- `IntHexMap` and `FloatHexMap` support cell-wise unary negation with `-`.
- `ToIntHexMap` and `ToFloatHexMap` create independent mutable copies of interface-typed maps.

All specialized maps inherit `HexMap<TValue>`, retain the same topology-backed indexing contract,
and return new maps from their operators without modifying the inputs.

```csharp
var topology = new HexMapTopology(4, 3, Layout.OddR);
var land = new BoolHexMap(topology, new bool[topology.Count]);
var landCost = new IntHexMap(topology, new int[topology.Count]);
var waterCost = new IntHexMap(topology, new int[topology.Count]);

IntHexMap movementCost = land.Select(landCost, waterCost);
IntHexMap adjustedCost = (movementCost + 2) * 3;
```

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
