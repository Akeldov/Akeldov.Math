# Complete and Partial Index Maps

Build two maps that store cell indices and visualize how they handle the map boundary.
<xref:Akeldov.Math.Hexes.Topology.IndexSeptupletMap> keeps all six logical neighbors, while
<xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletMap> marks which neighbors actually belong
to the bounded map.

These are hex maps: each value describes the neighborhood of a **hex cell**, not a raster pixel.
Both precompute the main index and six adjacent indices in the order defined by the layout.
Their geometry lets us rasterize them without supplying a separate placement.

Run each example separately in a console project that references Akeldov.Math.Hexes. If needed,
start with [Creating a Project](creating-a-hex-map/creating-a-project.md).

## Complete neighborhood: IndexSeptupletMap

Each cell stores a `Septuplet<VectorXYInt>`. Its `Main` is the cell's own index, and
`Adjacent0` through `Adjacent5` are its six logical neighbors. Boundary neighbors retain their
coordinates even when they lie outside the map. This is useful when an algorithm applies its own
boundary rules.

Create a 6 × 4 map with the `OddR` layout and a center-to-vertex radius of one world unit.
The red channel is the sum of the neighbors' X coordinates divided by `36f`; the green channel
uses their Y coordinates with the same divisor. Blue stays at full intensity. The main cell's
coordinates do not contribute.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using System.IO;

var geometry = new HexMapGeometry(
    width: 6,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var map = new IndexSeptupletMap(geometry);
Septuplet<VectorXYInt> neighborhood = map[new VectorXYInt(0, 0)];

map
    .Rasterize(
        pixelsPerApothem: 36f,
        margin: 0f,
        colorSelector: ToIndexColor)
    .SaveAsPng(Path.GetFullPath("index-septuplet-map.png"));

static RGBA16BitColor ToIndexColor(Septuplet<VectorXYInt> septuplet)
{
    float r =
        septuplet.Adjacent0.X +
        septuplet.Adjacent1.X +
        septuplet.Adjacent2.X +
        septuplet.Adjacent3.X +
        septuplet.Adjacent4.X +
        septuplet.Adjacent5.X;
    r /= 36f;

    float g =
        septuplet.Adjacent0.Y +
        septuplet.Adjacent1.Y +
        septuplet.Adjacent2.Y +
        septuplet.Adjacent3.Y +
        septuplet.Adjacent4.Y +
        septuplet.Adjacent5.Y;
    g /= 36f;

    return RGBA16BitColor.FromNormalized(r, g, 1f);
}
```

![Complete index map colored using all six adjacent indices](~/assets/hexes/topology/index-septuplet-map.png)

The divisor `36f` is a fixed color scale for this example, not an average over neighbors.
`FromNormalized` clamps channels to the range `0..1`, including negative contributions near the
boundary. The separate `pixelsPerApothem: 36f` controls image resolution, not color.

## Partial neighborhood: IndexPartialSeptupletMap

Each cell stores a `PartialSeptuplet<VectorXYInt>`. Its `HasAdjacent0` through
`HasAdjacent5` flags report which neighbors are inside the map. Check the corresponding flag
before using a neighbor to access another bounded map.

This example uses the same geometry and color scale, but only present neighbors contribute to
the coordinate sums:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using System.IO;

var geometry = new HexMapGeometry(
    width: 6,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var map = new IndexPartialSeptupletMap(geometry);
PartialSeptuplet<VectorXYInt> neighborhood = map[new VectorXYInt(0, 0)];

map
    .Rasterize(
        pixelsPerApothem: 36f,
        margin: 0f,
        colorSelector: ToIndexColor)
    .SaveAsPng(Path.GetFullPath("index-partial-septuplet-map.png"));

static RGBA16BitColor ToIndexColor(PartialSeptuplet<VectorXYInt> septuplet)
{
    float r =
        (septuplet.HasAdjacent0 ? septuplet.Adjacent0.X : 0) +
        (septuplet.HasAdjacent1 ? septuplet.Adjacent1.X : 0) +
        (septuplet.HasAdjacent2 ? septuplet.Adjacent2.X : 0) +
        (septuplet.HasAdjacent3 ? septuplet.Adjacent3.X : 0) +
        (septuplet.HasAdjacent4 ? septuplet.Adjacent4.X : 0) +
        (septuplet.HasAdjacent5 ? septuplet.Adjacent5.X : 0);
    r /= 36f;

    float g =
        (septuplet.HasAdjacent0 ? septuplet.Adjacent0.Y : 0) +
        (septuplet.HasAdjacent1 ? septuplet.Adjacent1.Y : 0) +
        (septuplet.HasAdjacent2 ? septuplet.Adjacent2.Y : 0) +
        (septuplet.HasAdjacent3 ? septuplet.Adjacent3.Y : 0) +
        (septuplet.HasAdjacent4 ? septuplet.Adjacent4.Y : 0) +
        (septuplet.HasAdjacent5 ? septuplet.Adjacent5.Y : 0);
    g /= 36f;

    return RGBA16BitColor.FromNormalized(r, g, 1f);
}
```

![Partial index map colored using only present adjacent indices](~/assets/hexes/topology/index-partial-septuplet-map.png)

## Compare the results

Interior cells have all six neighbors, so both examples produce the same colors there.
At the boundary, the complete map includes out-of-bounds coordinates in the sums; the partial
example skips them. The divisor remains `36f`, so the partial example does not renormalize the
remaining neighbors.

The partial map does not wrap or clamp missing neighbors to an existing cell. Its flags express
presence; they are not a replacement value. Both maps still require an in-bounds cell index when
reading the map itself.

See [Complete and Partial Neighborhoods](../concepts/data-storage/complete-and-partial-neighborhoods.md)
for the underlying value types, or [Rasterizing a Hex Map](rasterizing-a-hex-map/index.md) for
pixel-based index rasters and interpolation.
