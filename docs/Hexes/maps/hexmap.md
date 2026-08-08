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

- Higher-level APIs can build domain-specific maps on top of the same shape.
- The map type is not tied to topology, geometry, or chromatization values.

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
