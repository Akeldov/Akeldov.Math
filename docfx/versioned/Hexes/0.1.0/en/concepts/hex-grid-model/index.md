# Hex Grid Model

Akeldov.Math.Hexes separates the logical structure of a rectangular grid, its placement in
continuous space, and reusable finite shapes. Keeping these concerns in distinct values makes it
clear which operations depend on storage dimensions, physical scale, or only cell membership.

| Concern | Main type | Describes |
|---|---|---|
| Rectangular grid structure | <xref:Akeldov.Math.Hexes.HexMapTopology> | Resolution, cell count, and layout |
| Physical grid placement | <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> | Topology, zero-hex origin, radius, and apothem |
| Reusable finite shape | <xref:Akeldov.Math.Hexes.Topology.Polyhex> | An immutable cell mask in local Q/R coordinates |
| Shape construction | <xref:Akeldov.Math.Hexes.Topology.PolyhexBuilder> | A mutable mask that produces an immutable polyhex |

## Topology

<xref:Akeldov.Math.Hexes.HexMapTopology> is an immutable value that defines a rectangular set of
row-and-column indices. Its non-negative width and height determine which `VectorXYInt` values are
inside the map, while its <xref:Akeldov.Math.Hexes.Layout> determines how those indices correspond
to the hex lattice.

Topology does not store cell values or a physical hex size. Maps, neighborhood rasters, and
algorithms can therefore share the same topology without duplicating its structural parameters.

See [Topology](topology.md) for construction, bounds, equality, and layout consistency.

## Geometry

<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> adds the information required to place that
topology in Spatial2D coordinates: the world-space center of the zero hex and its radius. The
apothem is derived from the radius. The same logical topology can consequently be placed at
different origins or rendered at different scales.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 12,
    height: 8,
    layout: Layout.OddR);

var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 10f);
```

Use one geometry value whenever center calculations, map bounds, spatial sampling, or
rasterization must agree on topology and placement. See [Geometry](geometry.md) for origins,
dimensions, bounds, and spatial helpers.

## Polyhexes

A polyhex is a finite selection of cells stored as a rectangular Q/R mask. It is useful for local
shapes such as footprints, stamps, kernels, or game pieces. A polyhex does not contain a
<xref:Akeldov.Math.Hexes.Layout> or a world-space origin, so the same mask can be reused in
different grids and placements.

<xref:Akeldov.Math.Hexes.Topology.Polyhex> is immutable and compares by mask contents.
<xref:Akeldov.Math.Hexes.Topology.PolyhexBuilder> provides mutable construction, while
<xref:Akeldov.Math.Hexes.Geometry.PolyhexGeometry> associates the mask with a physical cell radius
and apothem without assigning a map layout or origin.

See [Polyhexes](polyhexes.md) for masks, indexing, builders, extension and contour operations, and
geometry wrappers.

## Keep the model boundaries explicit

- Keep a single topology with data that uses rectangular indices.
- Promote topology to geometry when an operation needs world-space positions or physical size.
- Use polyhexes for local Q/R shapes, and apply placement separately in the consuming operation.
- Pass the same layout through every conversion between QRS and row-and-column indices.

These model values describe structure rather than owning map data. Continue with
[Data Storage](../data-storage/index.md) to choose maps and rasters built on top of them, or return
to [Fundamentals](../fundamentals/index.md) for the coordinate and layout conventions they use.
