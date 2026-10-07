# Visualizing IndexTripletRaster

Turn the three hex indices stored at every raster sample into RGB channels.
<xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster> describes complete vertex neighborhoods,
including indices outside the finite source map. This example reproduces the index-color image
from the topology raster examples.

## What each pixel represents

The raster samples the world-space center of each pixel. It finds the containing hex and its
closest vertex, then stores a `Triplet<VectorXYInt>`:

- `Main`: the containing hex index, encoded in red.
- `Left`: one neighbor meeting it at that vertex, encoded in green.
- `Right`: the other neighbor at the vertex, encoded in blue.

`Left` and `Right` follow the layout's vertex ordering; they are not fixed screen directions.
These are **cell indices**, not barycentric weights or chromatic classes. Unlike an index map,
this raster stores one neighborhood per rectangular raster cell, not per hex.

## Render the complete triplets

Run this complete example in a console project that references Akeldov.Math.Hexes.
See [Creating a Project](creating-a-hex-map/creating-a-project.md) for setup.

The source map contains 5 × 4 hexes in the `OddR` layout, with a center-to-vertex radius of one
world unit. The constructor without an explicit origin places the zero hex at
`(apothem, radius)` for this layout. The raster starts at world point `(0, 0)`, spans the map's
bounding-box size, and samples it at 192 × 192 pixels.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.IO;

var hexMapGeometry = new HexMapGeometry(
    width: 5,
    height: 4,
    radius: 1f,
    layout: Layout.OddR);

var rasterGeometry = new RasterGeometry(
    new PointXY(0f, 0f),
    hexMapGeometry.GetBoundingBoxSize(),
    new VectorXYInt(192, 192));

var sourceRaster = new IndexTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("index-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(Triplet<VectorXYInt> triplet)
{
    return new RGBA16BitColor(
        ToChannel(EncodeIndex(triplet.Main)),
        ToChannel(EncodeIndex(triplet.Left)),
        ToChannel(EncodeIndex(triplet.Right)),
        ushort.MaxValue);
}

static float EncodeIndex(VectorXYInt index)
{
    return 0.08f + 0.075f * (index.X + 1) + 0.12f * (index.Y + 1);
}

static ushort ToChannel(float value)
{
    value = MathF.Min(MathF.Max(value, 0f), 1f);
    return (ushort)MathF.Round(value * ushort.MaxValue);
}
```

![IndexTripletRaster with Main, Left, and Right indices encoded in RGB](~/assets/hexes/rasters/index-triplet-raster-odd-r-rgba16.png)

## Read the result

`EncodeIndex` converts an index's X and Y coordinates into one intensity. `ToChannel` clamps
that intensity to `0..1`, then converts it to a 16-bit channel. This is a diagnostic color
mapping, not a unique or reversible encoding of an index.

The color changes when the containing hex or its closest vertex changes. It stays constant while
the selected triplet stays the same: this is not smooth barycentric interpolation.
`MapValues` converts the stored triplets to colors and preserves the sampling geometry.

The complete raster continues the logical hex grid beyond the finite map. An index outside the
source topology still contributes to its color channel. Clamping the color does **not** clamp the
index to the map boundary, so do not use an unchecked triplet to read a bounded `HexMap<T>`.

Compare this result with
[Visualizing IndexPartialTripletRaster](visualizing-index-partial-triplet-raster.md), which uses
the same sampling geometry and color formula but clears channels for absent indices.
For lookup and access details, see
[Create an Index Triplet Raster](../how-to-guides/rasters/create-an-index-triplet-raster.md).
