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

[Fundamentals](../fundamentals/index.md) covers [Topology](topology.md) and [Geometry](geometry.md).
These define the finite grid and its placement; polyhexes describe reusable cell shapes.

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
to [Fundamentals](../fundamentals/index.md) for coordinates, layouts, topology, and geometry.
