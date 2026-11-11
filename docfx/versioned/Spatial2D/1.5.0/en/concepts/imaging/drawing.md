# Drawing

Drawing adds shapes, curves, grids, and text to an existing `SpatialRaster<RGBA8BitColor>`.
The extension methods live in <xref:Akeldov.Math.Spatial2D.Imaging.SpatialRasterDrawingExtensions>.
Use them to annotate a sampled field, illustrate geometry, or build a diagram on a color buffer.

## Coordinates and the target raster

Drawing uses world coordinates, with Y increasing upwards. The raster's `Geometry` maps those
coordinates to pixels. Stroke widths, radii, dash lengths, arrow dimensions, grid spacing, and
text height are measured in world units. Increasing resolution adds detail without changing
the world-space dimensions of the drawing.

Each call modifies the existing pixel array in place. Constructors retain their arrays, so
other raster wrappers sharing that array see the changes. Copy `Values` before drawing when
the original image must remain unchanged. Geometry and resolution stay fixed.

## Available operations

| Content | Methods |
|---|---|
| Lines, rays, segments, arcs, and spline curves | `Draw`, `DrawSegment` |
| Closed contours and shape outlines | `Draw`, `DrawContour` |
| Dashed segments and contours | `DrawDashed`, `DrawDashedSegment`, dashed `DrawContour` overload |
| Filled circles and axis-aligned rectangles | `Fill`, `FillCircle`, `FillRectangle` |
| Arrows with solid or dashed shafts | `DrawArrow` |
| Coordinate grids, major lines, and optional labels | `DrawGrid` |
| Text with a built-in or supplied font | `DrawText` |

Curve strokes are centered on the curve and have round endpoints. Dashes and gaps have equal
nominal length; caps can narrow the visible gaps. Contour dash phase continues across edges.
Arrow heads remain solid even when their shafts are dashed. Grid lines are aligned to multiples
of the step from world zero, rather than from the raster origin.

## Coverage and compositing

Drawing is clipped to raster bounds. Curve strokes, circles, arrows, grids, and glyph masks use
4-by-4 coverage sampling; axis-aligned rectangle fills use exact cell overlap. Partial coverage
reduces the effective opacity at edges.

Colors are applied using source-over alpha compositing in call order. Later drawings appear
over earlier pixels, so drawing order matters for overlapping shapes. RGB compositing does not
apply gamma correction. The drawing methods target 8-bit RGBA spatial rasters; see
[Color Rasters](../rasterization/color-rasters.md) for other pixel formats.

## Text and reusable glyphs

`DrawText` places text relative to the first line's baseline, not its top-left corner.
The simplest overload draws black text with <xref:Akeldov.Math.Spatial2D.Imaging.GlyphSet.Default>.
Other overloads accept a color or a reusable <xref:Akeldov.Math.Spatial2D.Imaging.GlyphSet>
created from `PixelFont` or `TrueTypeFont`.

Reuse a glyph set across drawings to avoid repeated glyph preparation. `MeasureText` returns
logical bounds including spaces and line metrics, rather than exact visible glyph bounds.
Line breaks are supported; complex-script shaping, bidirectional layout, and tab expansion are
not performed. Mutable TrueType glyph caches are not intended for concurrent access.

For a complete example and parameter details, follow
[Draw Shapes and Text on a Raster](../../how-to-guides/rasterization/draw-on-a-raster.md).
See [Imaging](../imaging.md) for color representation, blending, and image export.
