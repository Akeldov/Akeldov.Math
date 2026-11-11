# FloatRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.FloatRaster> stores floating-point fields and distances without world-space bounds.

## Operations

Supports cell-wise `+`, binary `-`, and unary `-`. Scalar addition and subtraction work
on either side. Raster operands can be specialized or matching generic containers.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var resolution = new VectorXYInt(2, 2);
var raster = new FloatRaster(resolution, new[] { 1f, 2f, 3f, 4f });
FloatRaster shifted = raster + 0.5f;
FloatRaster difference = shifted - raster;
FloatRaster inverted = -raster;
```

## Storage and compatibility

The constructor retains its array. `Values` and the indexers expose mutable state. Operators
allocate new mutable results owned by the caller and leave their operands unchanged.

Binary raster operands must have equal resolution; there is no automatic resizing.

## Filtering and generation

`GaussianBlur(sigma)` or `GaussianBlur(sigma, radius)` returns a new `Raster<float>`.
Sigma and radius are measured in cells; edge weights are normalized over available cells.
`resolution.CreatePerlinNoise(seed: 42, scale: 10f)` generates a new `Raster<float>`
with values in `[0, 1]`. These methods return generic containers; wrap their `Values` in
`FloatRaster` when arithmetic operators are needed, remembering that the wrapper shares storage.

Noise scale is measured in raster cells.

See [Non-Spatial Rasters](../non-spatial-rasters.md).
