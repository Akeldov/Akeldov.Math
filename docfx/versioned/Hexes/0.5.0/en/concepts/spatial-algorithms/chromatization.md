# Chromatization

Chromatization assigns every hex index one of three classes: `0`, `1`, or `2`. This is a proper
coloring of the lattice: any two hexes that share an edge receive different classes, and the
three hexes incident to one vertex contain all three classes.

A class depends only on logical index and layout. Map values, finite map boundaries, hex radius,
and world-space origin do not affect it. The same classification can therefore drive independent
map passes and provide stable component order during interpolation.

## The Three-Color Invariant

In QRS coordinates, the class can be expressed as a remainder normalized modulo `3`:

```text
OddR and EvenR: class = mod3(Q - R)
OddQ and EvenQ: class = mod3(R - Q)
```

Here, `mod3` always returns `0`, `1`, or `2`, including for negative coordinates. Application
code does not need to reproduce this formula: the extension methods account for layout and work
directly with row-and-column indices.

<xref:Akeldov.Math.Hexes.Chromatization.VectorXYIntExtensions> provides:

- `GetChromaticClass(layout)` for any supported layout;
- `GetOddRChromaticClass()`, `GetEvenRChromaticClass()`, `GetOddQChromaticClass()`, and
  `GetEvenQChromaticClass()` for code whose layout is already known.

Passing a `Layout` value outside the four supported alternatives causes
`ArgumentOutOfRangeException`.

## One Index and a Class Map

Compute a class directly for a one-off query. When classes are read repeatedly across a finite
map, create <xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var index = new VectorXYInt(2, 1);
int computedClass = index.GetChromaticClass(topology.Layout);

var classes = new ChromaticIndexMap(topology);
byte storedClass = classes[index];

// computedClass == storedClass
```

`ChromaticIndexMap` precomputes one byte per cell and implements the read-only
`ISpatialHexMap<byte>` contract. Its <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> constructor
retains the supplied geometry. The topology-only constructor creates unit-radius geometry. In
both cases, the classes themselves depend only on resolution and layout.

## Geometric Triplet Order

Three hexes meet at each grid vertex. A regular
<xref:Akeldov.Math.Hexes.Topology.Triplet`1> stores them in geometric order:

- `Main` is the hex containing the sample point;
- `Left` and `Right` are its two neighbors at the nearest vertex.

`GetChromaticTriplet(layout)` converts a `Triplet<VectorXYInt>` into `Triplet<byte>` while
preserving this order. `Main`, `Left`, and `Right` become the class numbers of the corresponding
hexes. Because the three hexes are pairwise adjacent, the triplet contains `0`, `1`, and `2`, but
their positions depend on the selected vertex.

Do not treat `Main` as class `0`, `Left` as class `1`, or `Right` as class `2`.

## Chromatic Order

<xref:Akeldov.Math.Hexes.Topology.ChromaticTriplet`1> has a different contract. `Index0`,
`Index1`, and `Index2` correspond to classes `0`, `1`, and `2` regardless of the geometric
positions of the hexes. This order lets one channel consistently represent one chromatic class.

The distinction is especially important for barycentric weights:

| Raster type | Value | Order |
|---|---|---|
| <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexTripletRaster> | `Triplet<byte>` | Hex classes in `Main`, `Left`, `Right` order |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexPartialTripletRaster> | `PartialTriplet<byte>` | The same geometric order with presence flags |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster> | `ChromaticTriplet<float>` | Weights reordered as `Index0`, `Index1`, `Index2` |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster> | `PartialChromaticTriplet<float>` | The same weights with presence flags for each class |

For each raster cell, the algorithm uses the point at its center, finds the containing hex and
that hex's nearest vertex, and selects the containing hex plus the two neighbors incident to that
vertex. `SourceHexMapGeometry` defines the source grid, while `Geometry` and `Resolution` define
the separate sampling grid.

## Complete and Partial Rasters

Complete rasters extend the logical hex lattice beyond the finite map boundary. Every external
index still has a defined class, and barycentric coordinates can be calculated for a complete
triplet. Such an index cannot, however, be used safely to access a bounded value map.

Partial rasters compare every position with the source topology and retain a presence mask:

- `PartialTriplet<byte>` uses `HasMain`, `HasLeft`, and `HasRight`;
- `PartialChromaticTriplet<float>` uses `HasIndex0`, `HasIndex1`, and `HasIndex2`.

Check these flags before reading the corresponding value from another finite map. The stored
content of an absent position is not a substitute for its mask.

A raster can be constructed from only `HexMapGeometry` to cover the map automatically, or it can
receive a separate `RasterGeometry` for a specific sampling origin, size, and resolution. Source
map dimensions and raster resolution must be positive.

## When to Use Classes

- Process one class per pass when cells modified together must not share an edge.
- Use `Index0`, `Index1`, and `Index2` as stable channels when blending three values around a
  vertex.
- Use `ChromaticIndexMap` for visualization or repeated queries over a finite map.

The three-color invariant only guarantees that cells in one class are not direct edge neighbors.
If an operation affects cells beyond an immediate edge neighbor, chromatization alone may not be
sufficient for an independent parallel pass.

See [Complete and Partial Neighborhoods](../data-storage/complete-and-partial-neighborhoods.md)
for geometric and chromatic structure order, and [Rasters](../data-storage/rasters.md) for the
raster data model. Return to the [Spatial Algorithms overview](index.md) to compare
chromatization with pathfinding and Voronoi partitioning.
