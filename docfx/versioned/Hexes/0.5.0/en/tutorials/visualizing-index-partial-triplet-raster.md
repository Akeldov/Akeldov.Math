# Visualizing IndexPartialTripletRaster

Show how a finite hex-map boundary changes an index-triplet image.
<xref:Akeldov.Math.Hexes.Topology.IndexPartialTripletRaster> stores a
`PartialTriplet<VectorXYInt>` at each raster sample. Its `HasMain`, `HasLeft`, and `HasRight`
flags indicate which of the three selected hex indices belong to the source map.

This example uses the same geometry and index-color formula as
[Visualizing IndexTripletRaster](visualizing-index-triplet-raster.md). Only the treatment of
absent indices changes.

## Render only present indices

Run this complete example separately in a console project that references Akeldov.Math.Hexes.
See [Creating a Project](creating-a-hex-map/creating-a-project.md) for setup.

The source is a 5 × 4 `OddR` map with a center-to-vertex radius of one world unit. Its default
zero-hex center is `(apothem, radius)`. The sampling rectangle starts at world point `(0, 0)`,
spans the map's bounding-box size, and has a resolution of 192 × 192 pixels.

For each pixel, the raster selects the containing hex (`Main`) and the two neighbors
(`Left` and `Right`) meeting it at its closest vertex. Red, green, and blue encode these
three indices respectively. Each flag independently decides whether its channel contributes.

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

var sourceRaster = new IndexPartialTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("index-partial-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(PartialTriplet<VectorXYInt> triplet)
{
    return new RGBA16BitColor(
        ToPresenceChannel(triplet.Main, triplet.HasMain),
        ToPresenceChannel(triplet.Left, triplet.HasLeft),
        ToPresenceChannel(triplet.Right, triplet.HasRight),
        ushort.MaxValue);
}

static ushort ToPresenceChannel(VectorXYInt index, bool hasValue)
{
    return hasValue
        ? ToChannel(EncodeIndex(index))
        : (ushort)0;
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

![IndexPartialTripletRaster with absent indices represented by zero RGB channels](~/assets/hexes/rasters/index-partial-triplet-raster-odd-r-rgba16.png)

## Why the boundary looks different

Where all three selected hexes belong to the map, the colors match the complete raster.
Near the boundary, an absent `Main`, `Left`, or `Right` sets only its own RGB channel to zero.
This produces the saturated boundary colors in the image.

The example always sets alpha to `ushort.MaxValue`. Missing indices therefore do **not** make
a pixel transparent: a sample with no present indices is opaque black. A sample with an absent
`Main` can still display a present `Left` or `Right`; it is not discarded as a whole.

`EncodeIndex` is the same illustrative X/Y intensity formula as in the complete example.
`ToChannel` clamps and quantizes that intensity; it does not change which indices are present.
There is no barycentric weighting or renormalization.

## Use the presence flags when reading a map

A raster-cell coordinate and a stored hex index belong to different grids. A valid pixel can
contain a partial or entirely absent hex neighborhood. Check the relevant presence flag before
using a component to access a bounded source map.

Do not treat an index value of `(0, 0)` as an absence marker: it is also a valid hex index.
Presence is determined by `HasMain`, `HasLeft`, and `HasRight`, not by the stored coordinates.
The raster marks out-of-map positions absent; it does not wrap them around or replace them with
a boundary cell.

See [Handle Partial Neighborhoods](../how-to-guides/rasters/handle-partial-neighborhoods.md)
for boundary-safe value lookup. To interpolate values rather than display indices, continue with
[Create a Barycentric Raster](../how-to-guides/rasters/create-a-barycentric-raster.md).
