# Chromatization and Barycentric Interpolation

Combine the three-color classification of hexes with barycentric coordinates to blend colors
continuously between hex centers. Chromatization assigns each hex a class; barycentric
coordinates tell us how much each of three surrounding hexes contributes to a sample.

This tutorial continues [Chromatizing a Hex Map](chromatizing-a-hex-map.md). It reproduces the RGB
weight visualization used in the Hexes raster examples and adds the class ordering and boundary
rules needed to use it. The code runs in a console project that references Akeldov.Math.Hexes.

## From three hex centers to three weights

For each pixel center, the raster finds the containing hex and its nearest vertex. The centers
of the three hexes meeting at that vertex form a triangle. Barycentric coordinates express the
sample point as a weighted sum of those centers:

```text
P = wMain * centerMain + wLeft * centerLeft + wRight * centerRight
wMain + wLeft + wRight = 1
```

The triangle vertices here are **hex centers**, not the corners of an individual hex.
<xref:Akeldov.Math.Hexes.Topology.BarycentricTripletRaster> stores the weights in geometric
`Main/Left/Right` order. Those roles change as the sample moves through the grid.

The three hexes have distinct chromatic classes `0`, `1`, and `2`. Reordering the weights by
class gives every output channel a stable meaning:

| Geometric component | Example class | Example weight | Chromatic component |
| --- | --- | --- | --- |
| `Main` | `2` | `0.6` | `Index2 = 0.6` |
| `Left` | `0` | `0.1` | `Index0 = 0.1` |
| `Right` | `1` | `0.3` | `Index1 = 0.3` |

<xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster> performs this permutation.
Internally it combines `BarycentricTripletRaster` with `ChromaticIndexTripletRaster` on the same
sampling geometry. Its values are `ChromaticTriplet<float>`: `Index0`, `Index1`, and `Index2`
are **weights**, not cell indices or class numbers.

## Render class colors and blended weights

Assign red to class `0`, green to class `1`, and blue to class `2`. Then the blending formula
reduces to `RGB = (Index0, Index1, Index2)`. Run this complete example:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

RasterGeometry sampling = geometry.ToRasterGeometry(pixelsPerApothem: 32f);

var weights = new ChromaticBarycentricTripletRaster(geometry, sampling);
SpatialRaster<RGBA8BitColor> blendedImage = weights.MapValues(
    w => RGBA8BitColor.FromNormalized(w.Index0, w.Index1, w.Index2, alpha: 1f));

var classes = new ChromaticIndexMap(geometry);
SpatialRaster<RGBA8BitColor> classImage = classes.Rasterize(
    pixelsPerApothem: 32f,
    margin: 0f,
    colorSelector: c => RGBA8BitColor.FromNormalized(
        c == 0 ? 1f : 0f,
        c == 1 ? 1f : 0f,
        c == 2 ? 1f : 0f,
        alpha: 1f));

classImage.SaveAsPng(Path.GetFullPath("chromatic-classes.png"));
blendedImage.SaveAsPng(Path.GetFullPath("chromatic-barycentric.png"));
```

The first image paints each finite-map hex with its class color:

![Discrete RGB chromatic classes on a five-by-four OddR hex map](~/assets/hexes/chromatization/chromatic-classes.png)

The second image maps the three class-ordered weights directly to RGB:

![Barycentric RGB blending with red, green, and blue tied to chromatic classes](~/assets/hexes/chromatization/chromatic-barycentric.png)

At a hex center, its own weight is `1` and the other two are `0`. Halfway between two
neighboring centers, their weights are `0.5` each. At the common hex vertex, all three are
`1/3`, giving equal RGB channels and a gray color. Pixel centers usually only approximate these
exact positions.

The transitions are piecewise linear over the triangles of centers. Mapping
`Main/Left/Right` directly to RGB would instead attach channels to changing geometric roles;
the chromatic permutation keeps a given hex's contribution in the same channel.

## Use a different palette or values

For colors `C0`, `C1`, and `C2` assigned to the three classes, blend their components with:

```text
C(P) = Index0 * C0 + Index1 * C1 + Index2 * C2
```

The same formula interpolates numeric values. Class identifiers alone do not identify a unique
hex: many hexes share each class. If values vary per hex, obtain the actual surrounding indices
from `IndexTripletRaster`, look up their values, and reorder those values by class as well.
Alternatively, combine the index raster and ordinary barycentric raster directly in
`Main/Left/Right` order, as in
[Mapping Map Values to Pixels](rasterizing-a-hex-map/mapping-map-values-to-pixels.md).

## Handle the finite map boundary

The complete weight raster extends the logical hex lattice beyond the finite topology. This is
why the blended image fills the rectangular sampling area, including the corners where the
discrete class image is transparent. Complete weights still sum to approximately `1`.

For contributions from in-map hexes only, use
<xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster>. Add the following
after the example above:

```csharp
var partialWeights = new ChromaticBarycentricPartialTripletRaster(
    geometry, sampling);

SpatialRaster<RGBA8BitColor> finiteImage = partialWeights.MapValues(ToFiniteRgb);
finiteImage.SaveAsPng(Path.GetFullPath("chromatic-barycentric-finite.png"));

static RGBA8BitColor ToFiniteRgb(PartialChromaticTriplet<float> w)
{
    float w0 = w.HasIndex0 ? w.Index0 : 0f;
    float w1 = w.HasIndex1 ? w.Index1 : 0f;
    float w2 = w.HasIndex2 ? w.Index2 : 0f;
    float sum = w0 + w1 + w2;

    return sum > 0f
        ? RGBA8BitColor.FromNormalized(w0 / sum, w1 / sum, w2 / sum, alpha: 1f)
        : default;
}
```

![RGB blending using only present hexes with normalized boundary weights](~/assets/hexes/chromatization/chromatic-barycentric-finite.png)

The partial raster permutes both weights and presence flags into class order. It does not
renormalize the remaining weights. Dividing by their sum preserves the blend's intensity near
the boundary; leaving missing contributions at zero instead makes the RGB image darker there.
If no positive contribution remains, this example returns transparent black.

Presence flags describe the surrounding hexes, not a pixel clipping mask. A sample just outside
the finite map can still have an in-map neighbor. If an exact hex-shaped outline is required,
also check that the containing hex belongs to the source topology.

Reuse the same `HexMapGeometry` and `RasterGeometry` for related index, class, and weight
rasters: equal pixel counts alone do not guarantee that samples align. See
[Create a Chromatic Raster](../how-to-guides/chromatization/create-a-chromatic-raster.md) for
complete and partial raster APIs and
[Chromatization](../concepts/spatial-algorithms/chromatization.md) for the three-color invariant.
