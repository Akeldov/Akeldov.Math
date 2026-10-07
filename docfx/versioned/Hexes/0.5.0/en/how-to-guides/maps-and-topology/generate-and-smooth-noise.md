# Generate and Smooth Noise

Create deterministic procedural data with `CreatePerlinNoise`, then remove small-scale variation
with `GaussianBlur`.

## Generate a field

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(128, 96, Layout.OddR);

FloatHexMap height = topology.CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5,
    persistence: 0.5f,
    lacunarity: 2f,
    offset: new VectorXY(0f, 0f));
```

The result is a mutable `FloatHexMap` whose values lie in `[0, 1]`. The same arguments produce the
same field. Increase `scale` for broader features, and change `offset` to sample another region of
the same field—for example, an adjacent map chunk.

`scale`, `lacunarity`, and every offset component must be finite; scale and lacunarity must be
positive. `octaves` must be positive, and `persistence` must lie in `[0, 1]`.

## Smooth the field

```csharp
FloatHexMap smoothHeight = height.GaussianBlur(sigma: 1.25f);
```

`sigma` is measured in distances between edge-adjacent hex centers and must be finite and
positive. This overload truncates the kernel at three standard deviations.

Specify the kernel radius explicitly when work must be bounded:

```csharp
FloatHexMap compactBlur = height.GaussianBlur(sigma: 1.25f, radius: 2);
```

A radius of zero creates an independent copy. At map boundaries, the method normalizes weights
over available source cells, so it does not require padding. Both overloads leave `height`
unchanged.
