# Coordinates and Layouts

Use these recipes to convert coordinates between the logical QRS grid, rectangular storage,
and Akeldov.Math.Spatial2D space. The examples target Akeldov.Math.Hexes 0.1.0 and can be run
independently.

| Task | Recipe |
| --- | --- |
| Get a column and row index from an integer QRS coordinate | [Convert QRS to row and column indices](convert-qrs-to-row-and-column-indices.md) |
| Get QRS from a storage index | [Convert row and column indices to QRS](convert-row-and-column-indices-to-qrs.md) |
| Place QRS in continuous space | [Convert QRS to Spatial2D coordinates](convert-qrs-to-spatial2d-coordinates.md) |
| Identify a hex from a spatial point | [Find the nearest hex to a point](find-the-nearest-hex-to-a-point.md) |
| Select an integer hex for fractional QRS | [Round fractional QRS coordinates](round-fractional-qrs-coordinates.md) |
| Apply an exact or arbitrary rotation | [Rotate hex coordinates](rotate-hex-coordinates.md) |

Use the same <xref:Akeldov.Math.Hexes.Layout> for every conversion. For spatial conversions,
also retain the hex radius and the center of the zero hex: the same numeric coordinates describe
a different grid when these parameters change.

For a guided introduction, start with the
[Create a Hex Map tutorial](../../tutorials/creating-a-hex-map/index.md).
