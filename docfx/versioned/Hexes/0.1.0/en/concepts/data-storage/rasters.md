# Rasters

Hexes rasters are precomputed spatial lookup tables. For every cell of a rectangular sampling
grid, they store hex indices, barycentric weights, chromatic classes, or a combination of those
values. They implement the Spatial2D `ISpatialRaster<T>` contract, but their samples are semantic
data rather than image pixels.

This distinction matters: constructing an `IndexTripletRaster` does not draw a hex map. It builds
the information needed to look up or interpolate map values repeatedly. Producing a color or
grayscale image is a separate [rasterization](../rasterization.md) step.

## Source Geometry and Sampling Geometry

Every raster combines two independent geometries:

- <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> describes the source hex grid: its bounded
  <xref:Akeldov.Math.Hexes.HexMapTopology>, layout, zero-hex center, and hex radius;
- Spatial2D `RasterGeometry` describes the axis-aligned sampling rectangle: its lower-left
  `Origin`, world-space `Size`, integer `Resolution`, and derived `CellSize`.

The sample at raster coordinates `(x, y)` is evaluated at the center of that raster cell:

```text
sampleX = Geometry.Origin.X + (x + 0.5) * Geometry.CellSize.X
sampleY = Geometry.Origin.Y + (y + 0.5) * Geometry.CellSize.Y
```

Consequently, a raster's `Resolution` is `Geometry.Resolution`; it is not the hex map's
`Topology.Resolution`. One raster cell is one spatial sample, not one hex.

Each raster type has two constructors. Passing only the hex geometry uses
`hexMapGeometry.ToRasterGeometry(1f)`: the grid covers the complete map bounding box at a minimum
density of one sample per hex apothem, rounding the resolution up as needed. Pass an explicit
`RasterGeometry` to select a different rectangle or resolution:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var topology = new HexMapTopology(8, 6, Layout.OddR);
var source = new HexMapGeometry(
    topology,
    origin: new VectorXY(10f, 20f),
    radius: 2f);

RasterGeometry sampling = source.ToRasterGeometry(
    pixelsPerApothem: 4f,
    margin: 1f);

var raster = new IndexPartialTripletRaster(source, sampling);
```

Use the same `RasterGeometry` when several rasters must describe the same sample cells. Matching
only their resolutions is insufficient: their origins and sizes must match as well.

## Choose the Stored Sample

The package provides ten raster types in the `Akeldov.Math.Hexes.Topology` namespace. All support
the four <xref:Akeldov.Math.Hexes.Layout> values.

| Raster | Sample value | Meaning | `TryGetValue` |
| --- | --- | --- | --- |
| <xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster> | `Triplet<VectorXYInt>` | Main hex and the two hexes meeting it at the closest vertex | Yes |
| <xref:Akeldov.Math.Hexes.Topology.IndexPartialTripletRaster> | `PartialTriplet<VectorXYInt>` | Same triplet, clipped to the source topology | Yes |
| <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletRaster> | `Septuplet<VectorXYInt>` | Main hex and all six edge-adjacent hexes | No |
| <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster> | `PartialSeptuplet<VectorXYInt>` | Same logical indices, with in-bounds flags when `Main` is inside the source topology | No |
| <xref:Akeldov.Math.Hexes.Topology.BarycentricTripletRaster> | `Triplet<float>` | Weights for the main-left-right center triangle | Yes |
| <xref:Akeldov.Math.Hexes.Topology.BarycentricPartialTripletRaster> | `PartialTriplet<float>` | Same weights with out-of-map positions absent | Yes |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexTripletRaster> | `Triplet<byte>` | Chromatic classes of the main-left-right hexes | Yes |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexPartialTripletRaster> | `PartialTriplet<byte>` | Same classes with out-of-map positions absent | No |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster> | `ChromaticTriplet<float>` | Weights reordered by chromatic class 0, 1, and 2 | Yes |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster> | `PartialChromaticTriplet<float>` | Chromatically ordered weights with presence flags | Yes |

### Index Triplets

For every sample point, an index-triplet raster first classifies the main hex. It then selects the
closest vertex of that hex and records the other two hexes that meet the main hex at that vertex.
The result is ordered as `Main`, `Left`, `Right`; the order is layout-aware and is shared by the
index, barycentric, and chromatic-index triplet rasters.

Use `Main` when only the containing or nearest hex is needed. Keep all three positions when
interpolating values stored at hex centers:

```csharp
var indexRaster = new IndexTripletRaster(source, sampling);
var sampleIndex = new VectorXYInt(12, 7);

Triplet<VectorXYInt> indices = indexRaster[sampleIndex];
VectorXYInt main = indices.Main;
VectorXYInt left = indices.Left;
VectorXYInt right = indices.Right;
```

Complete triplets describe the infinite hex grid implied by the source geometry. Near or beyond
the finite map boundary, one or more returned indices can therefore be negative or greater than
or equal to the corresponding map-resolution component.

### Index Septuplets

An index-septuplet raster stores the classified `Main` hex followed by its six edge neighbors as
`Adjacent0` through `Adjacent5`. The adjacent positions follow the library's hex-edge order and
therefore depend on the layout when expressed as row-and-column indices.

Use septuplets for fixed seven-cell kernels, cellular rules, or neighborhood filters. They do not
represent the three-center triangle used for interpolation, and the package does not provide
barycentric or chromatic septuplet variants.

### Barycentric Triplets

A barycentric raster stores the coordinates of the sample point in the triangle formed by the
same `Main`, `Left`, and `Right` hex centers selected by an index-triplet raster. Pair rasters built
from identical source and sampling geometries:

```csharp
var indices = new IndexPartialTripletRaster(source, sampling);
var weights = new BarycentricPartialTripletRaster(source, sampling);
var sampleIndex = new VectorXYInt(12, 7);

PartialTriplet<VectorXYInt> cells = indices[sampleIndex];
PartialTriplet<float> barycentric = weights[sampleIndex];

if (cells.HasMain)
{
    VectorXYInt cell = cells.Main;
    float weight = barycentric.Main;
}
```

The three values of a complete barycentric triplet sum to approximately `1`, subject to
floating-point error. A partial raster preserves each surviving position's original weight and
sets absent positions to the default value; it does not renormalize the remaining weights.

### Chromatic Triplets

The hex grid has a repeating three-class chromatization. A `ChromaticIndexTripletRaster` stores
the class (`0`, `1`, or `2`) of the same `Main`, `Left`, and `Right` cells. These three cells meet
at one vertex, so a complete triplet contains all three classes.

A `ChromaticBarycentricTripletRaster` combines the classification and interpolation views. It
reorders the barycentric weights into `Index0`, `Index1`, and `Index2`, making the component
meaning stable even when the main-left-right order changes across the grid. The partial variant
reorders the presence flags along with the weights.

See [Chromatization](../spatial-algorithms/chromatization.md) for the class pattern and intended
uses.

## Complete and Partial Rasters

Complete rasters preserve the infinite-grid result. They are suitable when the caller has padded
data, deliberately samples outside the finite map, or wants geometric information independent of
map membership.

Partial rasters compare every referenced hex against the rectangular source topology:

```text
0 <= hexIndex.X < SourceHexMapGeometry.Topology.Resolution.X
0 <= hexIndex.Y < SourceHexMapGeometry.Topology.Resolution.Y
```

Presence is identified by flags, not by the stored payload. Partial triplet rasters store
`default(T)` in absent positions. `IndexPartialSeptupletRaster` instead retains every computed
logical index even when its flag is clear; when `Main` is outside the source topology, all seven
flags are clear even if one of the stored adjacent indices would be inside.

The available flags are:

- `PartialTriplet<T>` exposes `HasMain`, `HasLeft`, `HasRight`, and a `Presence` property of type
  `TripletPresenceFlags`;
- `PartialSeptuplet<T>` exposes `HasMain`, `HasAdjacent0` through `HasAdjacent5`, and
  a `Presence` property of type `SeptupletPresenceFlags`;
- `PartialChromaticTriplet<T>` exposes `HasIndex0`, `HasIndex1`, `HasIndex2`, and
  a `Presence` property of type `ChromaticTripletPresenceFlags`.

Never infer absence from the stored value. `(0, 0)` is a valid map index, chromatic class `0` is
valid, and a barycentric weight can legitimately be `0`. Check the corresponding `Has...`
property or `Presence` flag instead. Continue with
[Complete and Partial Neighborhoods](complete-and-partial-neighborhoods.md) for the shared
presence model.

## Index Raster Cells Safely

Raster instances expose `SourceHexMapGeometry`, `Geometry`, and `Resolution`, plus three read-only
indexers. `IndexTripletRaster` additionally exposes `Topology`; for every other raster, read it as
`SourceHexMapGeometry.Topology`.

```csharp
Triplet<VectorXYInt> byCoordinates = indexRaster[12, 7];
Triplet<VectorXYInt> byVector = indexRaster[new VectorXYInt(12, 7)];
Triplet<VectorXYInt> byFlatIndex = indexRaster[7 * indexRaster.Resolution.X + 12];
```

Flat indices use row-major order:

```text
flatIndex = y * Resolution.X + x
```

The `VectorXYInt` indexer checks both coordinates and throws `IndexOutOfRangeException` outside
the raster. The flat indexer follows array bounds. The `(x, y)` overload performs the row-major
calculation directly, so use it only after validating both coordinates; prefer the vector indexer
or an available `TryGetValue` method for untrusted indices.

`TryGetValue` is not uniform across partial types:

- complete triplet and chromatic-barycentric rasters return `true` for any index inside the
  sampling raster;
- `IndexPartialTripletRaster` and `BarycentricPartialTripletRaster` return `true` only when at
  least one source-map position is present;
- `ChromaticBarycentricPartialTripletRaster` checks raster bounds but can return `true` with
  `Presence == None`;
- septuplet rasters and `ChromaticIndexPartialTripletRaster` have no `TryGetValue`, so bounds-check
  first and inspect their presence flags after indexing.

These rules distinguish two different questions: whether a raster cell exists and whether that
cell refers to at least one hex inside the finite source map.

## Ownership and Cost

Construction allocates and eagerly fills an internal row-major array with
`Resolution.X * Resolution.Y` samples. The raster owns that storage and does not expose a backing
collection or setters. Indexers return read-only value structs, so callers cannot mutate the
precomputed raster through a returned sample.

Both construction time and retained memory scale with the number of sampling cells. The sample
type changes the per-cell size: a septuplet is larger than a triplet, and partial values also carry
presence flags. Reuse a raster when many queries share the same geometries; for a single point,
direct coordinate and adjacency helpers avoid the full precomputation.

## Validation and Boundary Behavior

Raster construction requires positive dimensions for both the source topology and sampling
resolution. An empty `HexMapTopology` is valid by itself but cannot be used to construct these
rasters. The allocation is checked and throws `OverflowException` if the total sampling-cell
count cannot fit an array length.

`RasterGeometry` validates its own finite origin, positive finite size, and positive resolution.
`ToRasterGeometry()` additionally requires a finite positive `pixelsPerApothem`, a finite
non-negative margin, a valid non-empty hex geometry, and a resulting resolution that fits in
`Int32`.

All four layouts use deterministic point-to-hex and closest-vertex classification. Samples
exactly on a shared boundary follow those helpers' tie rules; increasing resolution does not
change the rule, only which cell centers are sampled. See
[Coordinate Discretization](../fundamentals/coordinate-discretization.md) for those boundaries.

For focused construction examples, continue with
[Create an Index Triplet Raster](../../how-to-guides/rasters/create-an-index-triplet-raster.md),
[Create an Index Septuplet Raster](../../how-to-guides/rasters/create-an-index-septuplet-raster.md),
or [Create a Barycentric Raster](../../how-to-guides/rasters/create-a-barycentric-raster.md).
