# Draw Shapes and Text on a Raster

Use <xref:Akeldov.Math.Spatial2D.Imaging.SpatialRasterDrawingExtensions> to modify an existing
SpatialRaster&lt;RGBA8BitColor&gt;. Drawing uses world coordinates, with Y increasing upwards.
Widths, radii, dash lengths, grid spacing, arrow heads, and text height are floats in world units.

## Create and Draw

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    new PointXY(0, 0), new VectorXY(80, 50), new VectorXYInt(800, 500));
var pixels = new RGBA8BitColor[800 * 500];
Array.Fill(pixels, RGBA8BitColor.White);
var raster = new SpatialRaster<RGBA8BitColor>(geometry, pixels);
var blue = new RGBA8BitColor(37, 99, 235, 220);

raster.DrawGrid(new VectorXY(10, 10), width: 0.1f,
    color: new RGBA8BitColor(30, 41, 59, 35),
    majorEvery: 2, majorColor: new RGBA8BitColor(30, 41, 59, 90),
    labelHeight: 1f);
raster.FillCircle(new PointXY(20, 25), 8f, blue);
raster.FillRectangle(new PointXY(45, 15), new PointXY(65, 30), blue.Lighten(0.4f));
raster.DrawArrow(
    new ParameterizedSegment(new PointXY(10, 8), new PointXY(70, 8)),
    width: 0.3f, headLength: 3f, headWidth: 2f, dash: 2f,
    color: blue.Darken(0.3f));
raster.DrawText("Spatial2D 1.5", origin: new PointXY(10, 40), height: 3f);
raster.SaveAsPng("drawing.png");
```

The raster retains the supplied pixel array. Calls modify those pixels in place, in call order.
Drawing is clipped to raster bounds and uses source-over alpha compositing without gamma correction.

## Select an Operation

| Operation | API |
| --- | --- |
| Solid segment | DrawSegment(start, end, width, color), or Draw(Segment, width, color) |
| Dashed segment | DrawDashedSegment(start, end, width, dash, color), or DrawDashed(Segment, width, dash, color) |
| Other curves | Draw(curve, width, color), including parameterized curves through ICurve |
| Closed outline | Draw(contour, width, color), or DrawContour(points, width, color) |
| Dashed closed outline | DrawDashed(contour, width, color, dash), or DrawContour(points, width, color, dash) |
| Filled circle or rectangle | Fill(shape, color), FillCircle(center, radius, color), FillRectangle(cornerA, cornerB, color) |
| Arrow | DrawArrow(start, end, width, headLength, headWidth, color), or the ParameterizedSegment overload |
| Dashed arrow shaft | Add dash before color to either DrawArrow overload; zero selects a solid shaft |
| Coordinate grid | DrawGrid(step, width, color); the VectorXY overload also accepts major lines, labels, and bounds |

Curve strokes, arrows, grids, circles, and glyph masks use 4-by-4 coverage samples.
Axis-aligned rectangle fills use exact cell overlap. Stroke endpoints are round.

A dash and a gap have equal nominal length. Caps extend into gaps, so sufficiently thick strokes
can merge. Standalone dashed segments composite each dash separately; arrows and contours combine
their coverage before compositing. Contour dash phase continues across edges.

Arrow headLength is the axial distance from the tip to the head base; headWidth is the full
transverse span, excluding stroke thickness. Heads stay solid and keep their dimensions on short
arrows. Coincident endpoints draw nothing.

Grid lines lie at multiples of step from world zero, including negative coordinates. Major lines
take precedence at crossings. Labels use the built-in font and invariant numeric formatting;
overlapping candidates and labels that do not fit are skipped. An optional bounds rectangle
restricts both strokes and labels.

## Choose and Reuse a Font

DrawText(text, origin, height) uses black and the prebuilt <xref:Akeldov.Math.Spatial2D.Imaging.GlyphSet.Default>.
The color overload selects another color. The origin is the first baseline, not the top-left corner.

For a configurable pixel font or a TrueType file:

```csharp
var pixelGlyphs = new GlyphSet(new PixelFont(advance: 7, lineGap: 2), pixelHeight: 28);
raster.DrawText("Привет, Spatial2D!", pixelGlyphs, new PointXY(10, 35), 2f, blue);

var glyphs = new GlyphSet(TrueTypeFont.Load("MyFont.ttf"), pixelHeight: 48);
raster.DrawText("Cached text", glyphs, new PointXY(10, 30), 3f, blue);
var textBounds = glyphs.MeasureText("Cached text", height: 3f);
```

Provide the TrueType file yourself and reuse the same GlyphSet across drawings to avoid repeated
glyph preparation. TrueType masks are cached lazily; the default pixel glyph set is prebuilt.
Mutable TrueType caches are not intended for concurrent access.

MeasureText returns logical bounds relative to the baseline, including spaces and line metrics,
rather than exact visible glyph bounds. CR, LF, and CRLF line breaks are supported. Text drawing
does not perform complex-script shaping, bidirectional layout, or tab expansion. The pixel font
substitutes '?' for unsupported characters.

## Adjust Colors

```csharp
var border = blue.Darken(0.3f);
var light = blue.Lighten(0.4f, ColorAdjustmentMode.LinearRgb);
var tint = blue.Lighten(0.4f, ColorAdjustmentMode.Oklch);
```

These methods return a new RGBA8BitColor and preserve alpha. Input and output RGB are interpreted
as sRGB. The finite amount must be in [0, 1]; invalid values or modes throw ArgumentOutOfRangeException.
Zero preserves the color; one gives black or white with the original alpha.

Srgb adjusts encoded channels, LinearRgb adjusts decoded linear channels, and Oklch adjusts
lightness while retaining hue and reducing chroma only when needed to fit sRGB. Equal amounts
in different modes do not imply equal changes in luminance or perceived lightness.
