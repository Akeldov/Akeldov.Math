# Rasterization

Rasterization converts hex-map data into a rectangular Spatial2D sampling grid. Each result cell
stores a value obtained at one spatial point. That value can be an image color, a number for a
later calculation, or any other structure.

Akeldov.Math.Hexes provides three related but distinct operation groups:

| Task | Source | Result |
|---|---|---|
| Transfer logical map values into a specified resolution | `IHexMap<TValue>` | `Raster<TColor>` without world placement |
| Sample spatial map values on a world-space grid | `ISpatialHexMap<TValue>` | `SpatialRaster<TColor>` with `RasterGeometry` |
| Draw grid edges and labels | `HexMapGeometry` or `HexMapTopology` | `SpatialRaster<Gray8BitColor>` |

Index, barycentric, and chromatic rasters answer a different question: they precompute the
relationship between a sample point and nearby hexes. See
[Lookup Rasters and Ready Values](#lookup-rasters-and-ready-values) for the distinction.

## Sampling Model

When a map is rasterized, every output cell follows one sequence:

```text
raster cell → its center point → containing hex index
            → map value → colorSelector → result value
```

The algorithm selects a hex by the center point of the raster cell. It does not calculate pixel
coverage across several hexes, average neighboring values, or interpolate them. A value boundary
therefore runs between raster cells whose centers fall on opposite sides of a hex edge. Increasing
resolution makes this discretization visually finer.

If a center point maps to an index outside the finite topology, the result retains
`default(TColor)` and `colorSelector` is not called for that cell. This is especially visible in a
spatial raster with an outer `margin`.

`colorSelector` is a generic `Func<TValue, TColor>`. Its name reflects the common use case, but
`TColor` does not have to be a color. A `float` map can be rasterized into `byte`, another numeric
structure, or an actual Spatial2D color type.

## Output Raster Geometry

`RasterGeometry` describes a rectangular grid in world space:

- `Origin` and `Size` define the covered rectangle;
- `Resolution` defines the number of columns and rows;
- `CellSize` is derived from size and resolution;
- each sample point lies at the center of its cell.

<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> creates such a grid through
`ToRasterGeometry(pixelsPerApothem, margin)`. The grid covers the bounding rectangle of all hexes,
then adds the optional margin on every side.

`pixelsPerApothem` is the density in pixels per one hex apothem. Internally, it becomes one
world-unit density shared by both axes. Each resolution component is rounded up, so the actual
`CellSize` can be slightly smaller than the value implied directly by the requested density.
`margin` uses the same world units as geometry radius, origin, and size.

The same operation is available on a spatial map through `map.ToRasterGeometry(...)`, or on a
topology when radius and origin are supplied explicitly. Reuse one geometry when several rasters
must have exactly matching sample cells.

## Logical Map Without Geometry

<xref:Akeldov.Math.Hexes.IHexMap`1> stores topology but not radius or origin. The overload

```csharp
Raster<TColor> Rasterize(
    VectorXYInt resolution,
    Func<TValue, TColor> colorSelector)
```

temporarily places the topology at the default origin with unit radius, covers its bounding box,
and samples into the specified `resolution`. The resulting `Raster<TColor>` has a resolution and
a new mutable caller-owned `Values` array, but it does not retain world geometry.

Use this form for a logical preview where layout shape and final pixel dimensions matter. If the
map implements <xref:Akeldov.Math.Hexes.ISpatialHexMap`1> and its real placement matters, use the
spatial overloads.

## Spatial Map

An `ISpatialHexMap<TValue>` offers two ways to define the result grid:

- pass `pixelsPerApothem` and an optional `margin` to cover the complete map automatically;
- pass a ready `RasterGeometry` to sample an arbitrary region and resolution.

Both forms return `SpatialRaster<TColor>` and retain the chosen result geometry. A custom grid can
cover only part of the map, extend beyond it, or start somewhere other than the map bounding box.

The following example converts a terrain-type map into an RGBA16 image. The outer margin stays
transparent because `default(RGBA16BitColor)` has zero-valued channels.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

var terrain = new SpatialHexMap<int>(geometry, new[]
{
    0, 0, 1, 1,
    0, 1, 1, 2,
    1, 1, 2, 2
});

SpatialRaster<RGBA16BitColor> image = terrain.Rasterize(
    pixelsPerApothem: 24f,
    margin: geometry.Radius,
    colorSelector: TerrainColor);

image.SaveAsPng("terrain.png");

static RGBA16BitColor TerrainColor(int terrainType)
{
    return terrainType switch
    {
        0 => new RGBA16BitColor(0x2800, 0x6800, 0xd800, ushort.MaxValue),
        1 => new RGBA16BitColor(0x3200, 0xa000, 0x4800, ushort.MaxValue),
        2 => new RGBA16BitColor(0x8800, 0x8800, 0x8800, ushort.MaxValue),
        _ => new RGBA16BitColor(0xe000, 0x3000, 0x3000, ushort.MaxValue)
    };
}
```

`Rasterize` creates raster data, while Spatial2D APIs determine the file format. `SaveAsPng` is
available for supported color rasters; a generic `Raster<T>` is not inherently an image.

## Map Edges

Geometry rasterization draws the unique edge segments of all hexes. An edge shared by two
neighboring cells is processed once. `HexMapGeometry.Rasterize(...)` accepts line width, fade
distance, edge color, background color, and an integer `pixelsPerApothem` density.

```csharp
SpatialRaster<Gray8BitColor> edges = geometry.Rasterize(
    curveWidth: 0.08f,
    fadeDistance: 0.05f,
    curveColor: Gray8BitColor.Black,
    backgroundColor: Gray8BitColor.White,
    pixelsPerApothem: 24);

edges.SaveAsPng("hex-edges.png");
```

Line width and fade distance use world units rather than pixels. The same parameters therefore
produce the same physical thickness at different resolutions, but a different number of pixels
across the transition.

## Topology and Coordinate Labels

A topology can be drawn without constructing `HexMapGeometry` first. `Rasterize` accepts radius,
an optional zero-hex origin, and
<xref:Akeldov.Math.Hexes.HexMapTopologyRasterizationOptions>:

| Parameter | Purpose | Unit |
|---|---|---|
| `Margin` | Outer padding on every side | World coordinates |
| `CurveWidth` | Edge width | World coordinates |
| `FadeDistance` | Distance of the transition to the background | World coordinates |
| `CurveColor` | Value at the edge center | `Gray8BitColor` |
| `BackgroundColor` | Value outside the fade region | `Gray8BitColor` |
| `PixelsPerApothem` | Result density | Pixels per apothem |

Without the overload that accepts `origin`, the zero hex is centered at `VectorXY.Zero`.

Coordinate labels can be added to a topology raster:

- <xref:Akeldov.Math.Hexes.HexMapTopologyXYLabelsRasterizationOptions> renders `(X, Y)`;
- <xref:Akeldov.Math.Hexes.HexMapTopologyQRSLabelsRasterizationOptions> converts each index using
  the layout and renders `(Q, R, S)`;
- the overload that accepts both option values overlays both layers.

Each label option receives a `TrueTypeFont`, size, color, edge falloff, and optional offset from
the hex center. Size, falloff, and offset use world units. The caller loads and supplies the font.

## Lookup Rasters and Ready Values

`IndexTripletRaster`, `IndexSeptupletRaster`, `BarycentricTripletRaster`, and their partial or
chromatic variants already implement `ISpatialRaster<TValue>`. They store neighbor indices,
weights, or classes calculated for every sample point rather than the color of one map cell.

`MapValues(selector)` converts each already stored value into a new type while preserving the
same `RasterGeometry`. It does not perform another spatial sampling step. For example, a
barycentric raster can be converted to color without repeating the nearest-vertex search.

Use:

- `map.Rasterize(...)` when each point needs one containing hex and that hex's map value;
- a specialized raster when several neighbors, interpolation weights, or presence flags are
  required;
- `MapValues(...)` when spatial relationships are already calculated and only value
  representation must change.

Raster families are described in [Rasters](data-storage/rasters.md), and complete versus partial
neighborhood behavior in
[Complete and Partial Neighborhoods](data-storage/complete-and-partial-neighborhoods.md).

## Parameter Validation

- Output raster resolution and source-map dimensions must be positive.
- `pixelsPerApothem` must be finite and positive.
- `margin` must be finite and non-negative.
- Geometry origin and size must remain finite, while radius and apothem must be positive.
- `colorSelector` cannot be `null`.
- If the product of resolution components or a calculated resolution does not fit in `Int32`, an
  `OverflowException` is thrown.

Every raster-producing operation creates a new result array. Mutating it does not change the
source map or a specialized source raster.

Return to [Data Storage](data-storage/index.md) to choose a source map or raster, or to
[Spatial Algorithms](spatial-algorithms/index.md) to see which results can be visualized this way.
