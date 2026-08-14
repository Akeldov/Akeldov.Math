# Coordinate Systems

Akeldov.Math.Hexes uses different coordinate types for grid arithmetic, rectangular storage, and
continuous geometry. The same hex can therefore have a QRS coordinate, a row-and-column index,
and a world-space center. These values are related, but they are not interchangeable.

## Coordinate representations

| Representation | Main types | Meaning |
|---|---|---|
| Integer QRS | <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> | A discrete hex coordinate or grid offset |
| Fractional QRS | <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> | A continuous value expressed along the three hex-grid axes |
| Row and column | `VectorXYInt` | An index into a rectangular map or array |
| Spatial | `PointXY` and `VectorXY` | Positions and offsets in continuous world space |

QRS coordinates describe the logical hex lattice without storing row or column staggering.
`VectorXYInt` uses ordinary integer `X` and `Y` components, but their interpretation as a hex
index depends on <xref:Akeldov.Math.Hexes.Layout>. Spatial values already identify positions or
offsets. Relating them to the grid additionally requires its layout and radius and, for positions,
the center of the zero hex.

None of these value types stores a layout. APIs request the layout at every conversion where
orientation or offset parity matters.

## Choose coordinates by purpose

Use QRS coordinates while an operation is about the hex lattice itself:

- additions and subtractions of grid offsets;
- grid-distance calculations;
- exact rotations in 60-degree steps;
- continuous calculations that will later be rounded to a hex.

Use row-and-column indices at the storage boundary. <xref:Akeldov.Math.Hexes.HexMapTopology> and
`HexMap<T>` describe rectangular storage with `X` as the column, `Y` as the row, and a
<xref:Akeldov.Math.Hexes.Layout> that explains how those rows or columns are staggered.

Use spatial coordinates for centers, vertices, bounds, rendering, hit testing, and integration
with Akeldov.Math.Spatial2D geometry.

## Understand the conversion boundary

Converting a <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> to `VectorXYInt` and back is exact
when both calls use the same layout. Changing the layout can change the row-and-column index.

Conversions between fractional QRS and `VectorXY` use a unit-radius hex basis selected by the
layout orientation. Physical centers and offsets also use the hex radius and, when a position
rather than an offset is required, the center of the zero hex.

Discretization is different from conversion: it chooses one cell from a continuous value and is
therefore lossy. Use the dedicated nearest-hex and point-to-hex methods instead of numeric casts.

## Topics

- [QRS Coordinates](qrs-coordinates.md) — the zero-sum invariant, integer and fractional values,
  arithmetic, and distance.
- [Row and Column Indices](row-and-column-indices.md) — rectangular storage, layout-aware
  conversion, bounds, and adjacency.
- [Spatial Coordinates](spatial-coordinates.md) — world-space axes, radius, origin, centers, and
  QRS/XY geometry conversion.
- [Layouts](../layouts.md) — pointy-top and flat-top orientations and odd/even offset rules.
- [Coordinate Discretization](../coordinate-discretization.md) — rounding fractional QRS values
  and locating the hex that contains a world-space point.
